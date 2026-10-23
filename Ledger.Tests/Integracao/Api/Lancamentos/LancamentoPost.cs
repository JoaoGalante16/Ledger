using System.Net;
using System.Net.Http.Json;
using Ledger.Application.Dtos.Contas;
using Ledger.Application.Dtos.Lancamentos;
using Ledger.Tests.Integracao.Api.Suporte;
using Microsoft.AspNetCore.Mvc;

namespace Ledger.Tests.Integracao.Api.Lancamentos;

[Collection(nameof(ApiCollection))]
public class LancamentoPost : IAsyncLifetime
{
    private readonly LedgerWebApplicationFactory _app;

    public LancamentoPost(LedgerWebApplicationFactory app)
    {
        _app = app;
    }

    public async ValueTask InitializeAsync() => await _app.LimparBancoAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task TransferenciaGravaOParEAtualizaOsSaldos()
    {
        using var joao = await _app.CriarClienteAutenticadoAsync("joao@teste.com");
        using var maria = await _app.CriarClienteAutenticadoAsync("maria@teste.com");
        var origem = await _app.CriarContaAsync(joao);
        var destino = await _app.CriarContaAsync(maria);
        await _app.DepositarAsync(origem.Numero, 500m);

        var resposta = await joao.PostAsJsonAsync("/Lancamento",
            new { numeroContaOrigem = origem.Numero, numeroContaDestino = destino.Numero, valor = 120.50m, descricao = "Aluguel" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var par = await resposta.Content.ReadFromJsonAsync<List<ReadLancamentoDto>>(TestContext.Current.CancellationToken);
        Assert.NotNull(par);
        Assert.Equal(2, par.Count);
        Assert.Equal(par[0].IdTransacao, par[1].IdTransacao);
        Assert.Equal(new[] { -120.50m, 120.50m }, par.Select(l => l.Valor));

        var saldoOrigem = await joao.GetFromJsonAsync<ReadContaDto>($"/Conta/{origem.Numero}", TestContext.Current.CancellationToken);
        var saldoDestino = await maria.GetFromJsonAsync<ReadContaDto>($"/Conta/{destino.Numero}", TestContext.Current.CancellationToken);
        Assert.Equal(379.50m, saldoOrigem!.Saldo);
        Assert.Equal(120.50m, saldoDestino!.Saldo);
    }

    [Fact]
    public async Task SaldoInsuficienteDevolve400()
    {
        using var client = await _app.CriarClienteAutenticadoAsync("joao@teste.com");
        var origem = await _app.CriarContaAsync(client);
        var destino = await _app.CriarContaAsync(client, "Poupança");
        await _app.DepositarAsync(origem.Numero, 50m);

        var resposta = await client.PostAsJsonAsync("/Lancamento",
            new { numeroContaOrigem = origem.Numero, numeroContaDestino = destino.Numero, valor = 100m },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        var problema = await resposta.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);
        Assert.Equal("Saldo insuficiente", problema!.Detail);
    }

    [Fact]
    public async Task ContaDeDestinoEncerradaDevolve409()
    {
        using var client = await _app.CriarClienteAutenticadoAsync("joao@teste.com");
        var origem = await _app.CriarContaAsync(client);
        var destino = await _app.CriarContaAsync(client, "Poupança");
        await _app.DepositarAsync(origem.Numero, 100m);
        await client.PostAsync($"/Conta/{destino.Numero}/encerrar", null, TestContext.Current.CancellationToken);

        var resposta = await client.PostAsJsonAsync("/Lancamento",
            new { numeroContaOrigem = origem.Numero, numeroContaDestino = destino.Numero, valor = 10m },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
        var problema = await resposta.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);
        Assert.Equal("A conta de destino foi encerrada", problema!.Detail);
    }

    [Fact]
    public async Task ValorNegativoDevolve400ComOErroNoCampo()
    {
        using var client = await _app.CriarClienteAutenticadoAsync("joao@teste.com");
        var origem = await _app.CriarContaAsync(client);
        var destino = await _app.CriarContaAsync(client, "Poupança");

        var resposta = await client.PostAsJsonAsync("/Lancamento",
            new { numeroContaOrigem = origem.Numero, numeroContaDestino = destino.Numero, valor = -50m },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        var problema = await resposta.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestContext.Current.CancellationToken);
        Assert.True(problema!.Errors.ContainsKey("Valor"));
    }
}
