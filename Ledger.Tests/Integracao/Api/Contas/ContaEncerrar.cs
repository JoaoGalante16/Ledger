using System.Net;
using System.Net.Http.Json;
using Ledger.Application.Dtos.Contas;
using Ledger.Domain.Constantes;
using Ledger.Tests.Integracao.Api.Suporte;
using Microsoft.AspNetCore.Mvc;

namespace Ledger.Tests.Integracao.Api.Contas;

[Collection(nameof(ApiCollection))]
public class ContaEncerrar : IAsyncLifetime
{
    private readonly LedgerWebApplicationFactory _app;

    public ContaEncerrar(LedgerWebApplicationFactory app)
    {
        _app = app;
    }

    public async ValueTask InitializeAsync() => await _app.LimparBancoAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task EncerraContaComSaldoZero()
    {
        using var client = await _app.CriarClienteAutenticadoAsync("joao@teste.com");
        var conta = await _app.CriarContaAsync(client);

        var resposta = await client.PostAsync($"/Conta/{conta.Numero}/encerrar", null, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        var contaEncerrada = await client.GetFromJsonAsync<ReadContaDto>($"/Conta/{conta.Numero}", TestContext.Current.CancellationToken);
        Assert.NotNull(contaEncerrada!.DataEncerramento);
    }

    [Fact]
    public async Task ContaComSaldoDevolve409()
    {
        using var client = await _app.CriarClienteAutenticadoAsync("joao@teste.com");
        var conta = await _app.CriarContaAsync(client);
        await _app.DepositarAsync(conta.Numero, 10m);

        var resposta = await client.PostAsync($"/Conta/{conta.Numero}/encerrar", null, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
        var problema = await resposta.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);
        Assert.Equal("Existe saldo disponível na conta", problema!.Detail);
    }

    [Fact]
    public async Task AdminNaoEncerraAContaCaixa()
    {
        using var admin = await _app.CriarClienteAdminAsync("admin@teste.com");
        await using var contexto = _app.CriarContexto();
        var caixa = contexto.Contas.Single(c => c.IdUsuario == ContaCaixa.IdUsuario);

        var resposta = await admin.PostAsync($"/Conta/{caixa.Numero}/encerrar", null, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
        var problema = await resposta.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);
        Assert.Equal("Não é possível encerrar a conta caixa", problema!.Detail);
    }

    [Fact]
    public async Task EncerrarEDepositarAoMesmoTempoNaoDeixaContaEncerradaComSaldo()
    {
        var cancelamento = TestContext.Current.CancellationToken;
        using var joao = await _app.CriarClienteAutenticadoAsync("joao@teste.com");
        var contas = new List<ReadContaDto>();
        for (var i = 0; i < 10; i++)
            contas.Add(await _app.CriarContaAsync(joao));

        var operacoes = contas.SelectMany(conta => new[]
        {
            joao.PostAsJsonAsync("/Lancamento/deposito", new { numeroConta = conta.Numero, valor = 10m }, cancelamento),
            joao.PostAsync($"/Conta/{conta.Numero}/encerrar", null, cancelamento)
        });
        await Task.WhenAll(operacoes);

        await using var contexto = _app.CriarContexto();
        var numeros = contas.Select(c => c.Numero).ToList();
        var encerradasComSaldo = contexto.Contas
            .Where(c => numeros.Contains(c.Numero))
            .AsEnumerable()
            .Count(c => c.DataEncerramento != null && c.Saldo.Value != 0);
        Assert.Equal(0, encerradasComSaldo);
    }
}
