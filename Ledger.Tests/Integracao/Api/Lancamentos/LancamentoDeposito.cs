using System.Net;
using System.Net.Http.Json;
using Ledger.Application.Dtos.Contas;
using Ledger.Application.Dtos.Lancamentos;
using Ledger.Tests.Integracao.Api.Suporte;

namespace Ledger.Tests.Integracao.Api.Lancamentos;

[Collection(nameof(ApiCollection))]
public class LancamentoDeposito : IAsyncLifetime
{
    private readonly LedgerWebApplicationFactory _app;

    public LancamentoDeposito(LedgerWebApplicationFactory app)
    {
        _app = app;
    }

    public async ValueTask InitializeAsync() => await _app.LimparBancoAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task DepositoAumentaOSaldoDaConta()
    {
        using var joao = await _app.CriarClienteAutenticadoAsync("joao@teste.com");
        var conta = await _app.CriarContaAsync(joao);

        var resposta = await joao.PostAsJsonAsync("/Lancamento/deposito",
            new { numeroConta = conta.Numero, valor = 250m },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var par = await resposta.Content.ReadFromJsonAsync<List<ReadLancamentoDto>>(TestContext.Current.CancellationToken);
        Assert.Equal(2, par!.Count);
        Assert.All(par, l => Assert.Equal("Deposito", l.TipoOperacao));

        var contaDepois = await joao.GetFromJsonAsync<ReadContaDto>($"/Conta/{conta.Numero}", TestContext.Current.CancellationToken);
        Assert.Equal(250m, contaDepois!.Saldo);
    }

    [Fact]
    public async Task DepositoEmContaDeOutroUsuarioDevolve404()
    {
        using var joao = await _app.CriarClienteAutenticadoAsync("joao@teste.com");
        using var maria = await _app.CriarClienteAutenticadoAsync("maria@teste.com");
        var contaDaMaria = await _app.CriarContaAsync(maria);

        var resposta = await joao.PostAsJsonAsync("/Lancamento/deposito",
            new { numeroConta = contaDaMaria.Numero, valor = 50m },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task CorrecaoDeDepositoDevolve409()
    {
        using var joao = await _app.CriarClienteAutenticadoAsync("joao@teste.com");
        using var admin = await _app.CriarClienteAdminAsync("admin@teste.com");
        var conta = await _app.CriarContaAsync(joao);
        var deposito = await joao.PostAsJsonAsync("/Lancamento/deposito",
            new { numeroConta = conta.Numero, valor = 100m },
            TestContext.Current.CancellationToken);
        var par = await deposito.Content.ReadFromJsonAsync<List<ReadLancamentoDto>>(TestContext.Current.CancellationToken);

        var resposta = await admin.PostAsJsonAsync("/Lancamento/correcao",
            new { valor = 100m, descricao = "Estorno", idLancamentoReferencia = par![0].IdTransacao },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
    }
}
