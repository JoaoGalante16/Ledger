using System.Net;
using System.Net.Http.Json;
using Ledger.Application.Dtos.Contas;
using Ledger.Tests.Integracao.Api.Suporte;
using Microsoft.AspNetCore.Mvc;

namespace Ledger.Tests.Integracao.Api.Lancamentos;

[Collection(nameof(ApiCollection))]
public class LancamentoSaque : IAsyncLifetime
{
    private readonly LedgerWebApplicationFactory _app;

    public LancamentoSaque(LedgerWebApplicationFactory app)
    {
        _app = app;
    }

    public async ValueTask InitializeAsync() => await _app.LimparBancoAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task SaqueDiminuiOSaldoDaConta()
    {
        using var joao = await _app.CriarClienteAutenticadoAsync("joao@teste.com");
        var conta = await _app.CriarContaAsync(joao);
        await joao.PostAsJsonAsync("/Lancamento/deposito",
            new { numeroConta = conta.Numero, valor = 300m },
            TestContext.Current.CancellationToken);

        var resposta = await joao.PostAsJsonAsync("/Lancamento/saque",
            new { numeroConta = conta.Numero, valor = 120.50m },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var contaDepois = await joao.GetFromJsonAsync<ReadContaDto>($"/Conta/{conta.Numero}", TestContext.Current.CancellationToken);
        Assert.Equal(179.50m, contaDepois!.Saldo);
    }

    [Fact]
    public async Task SaqueSemSaldoDevolve400()
    {
        using var joao = await _app.CriarClienteAutenticadoAsync("joao@teste.com");
        var conta = await _app.CriarContaAsync(joao);

        var resposta = await joao.PostAsJsonAsync("/Lancamento/saque",
            new { numeroConta = conta.Numero, valor = 10m },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        var problema = await resposta.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);
        Assert.Equal("Saldo insuficiente", problema!.Detail);
    }
}
