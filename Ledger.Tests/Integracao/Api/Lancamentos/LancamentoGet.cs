using System.Net;
using System.Net.Http.Json;
using Ledger.Data.Dtos.LancamentoDtos;
using Ledger.Tests.Integracao.Api.Suporte;

namespace Ledger.Tests.Integracao.Api.Lancamentos;

[Collection(nameof(ApiCollection))]
public class LancamentoGet : IAsyncLifetime
{
    private readonly LedgerWebApplicationFactory _app;

    public LancamentoGet(LedgerWebApplicationFactory app)
    {
        _app = app;
    }

    public async ValueTask InitializeAsync() => await _app.LimparBancoAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task BuscaLancamentoDaPropriaConta()
    {
        using var client = await _app.CriarClienteAutenticadoAsync("joao@teste.com");
        var conta = await _app.CriarContaAsync(client);
        var deposito = await _app.DepositarAsync(conta.Numero, 80m);

        var resposta = await client.GetFromJsonAsync<ReadLancamentoDto>($"/Lancamento/{deposito.Id}", TestContext.Current.CancellationToken);

        Assert.NotNull(resposta);
        Assert.Equal(80m, resposta.Valor);
        Assert.Equal(conta.Numero, resposta.NumeroConta);
    }

    [Fact]
    public async Task LancamentoDeOutroUsuarioDevolve404()
    {
        using var joao = await _app.CriarClienteAutenticadoAsync("joao@teste.com");
        using var maria = await _app.CriarClienteAutenticadoAsync("maria@teste.com");
        var contaDaMaria = await _app.CriarContaAsync(maria);
        var deposito = await _app.DepositarAsync(contaDaMaria.Numero, 80m);

        var resposta = await joao.GetAsync($"/Lancamento/{deposito.Id}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task ListaSoOsLancamentosDasContasDoUsuario()
    {
        using var joao = await _app.CriarClienteAutenticadoAsync("joao@teste.com");
        using var maria = await _app.CriarClienteAutenticadoAsync("maria@teste.com");
        var contaDoJoao = await _app.CriarContaAsync(joao);
        var contaDaMaria = await _app.CriarContaAsync(maria);
        await _app.DepositarAsync(contaDoJoao.Numero, 10m);
        await _app.DepositarAsync(contaDoJoao.Numero, 20m);
        await _app.DepositarAsync(contaDaMaria.Numero, 30m);

        var lancamentos = await joao.GetFromJsonAsync<List<ReadLancamentoDto>>("/Lancamento", TestContext.Current.CancellationToken);

        Assert.NotNull(lancamentos);
        Assert.Equal(2, lancamentos.Count);
        Assert.All(lancamentos, l => Assert.Equal(contaDoJoao.Numero, l.NumeroConta));
    }

    [Fact]
    public async Task AdminListaOsLancamentosDeTodos()
    {
        using var joao = await _app.CriarClienteAutenticadoAsync("joao@teste.com");
        using var maria = await _app.CriarClienteAutenticadoAsync("maria@teste.com");
        using var admin = await _app.CriarClienteAdminAsync("admin@teste.com");
        var contaDoJoao = await _app.CriarContaAsync(joao);
        var contaDaMaria = await _app.CriarContaAsync(maria);
        await _app.DepositarAsync(contaDoJoao.Numero, 10m);
        await _app.DepositarAsync(contaDaMaria.Numero, 30m);

        var lancamentos = await admin.GetFromJsonAsync<List<ReadLancamentoDto>>("/Lancamento", TestContext.Current.CancellationToken);

        Assert.NotNull(lancamentos);
        Assert.Equal(new[] { contaDoJoao.Numero, contaDaMaria.Numero }.Order(), lancamentos.Select(l => l.NumeroConta).Order());
    }
}
