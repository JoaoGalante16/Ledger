using System.Net;
using Ledger.Tests.Integracao.Api.Suporte;

namespace Ledger.Tests.Integracao.Api.Contas;

[Collection(nameof(ApiCollection))]
public class ContaDelete : IAsyncLifetime
{
    private readonly LedgerWebApplicationFactory _app;

    public ContaDelete(LedgerWebApplicationFactory app)
    {
        _app = app;
    }

    public async ValueTask InitializeAsync() => await _app.LimparBancoAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task ExcluiContaSemLancamentos()
    {
        using var client = await _app.CriarClienteAutenticadoAsync("joao@teste.com");
        var conta = await _app.CriarContaAsync(client);

        var resposta = await client.DeleteAsync($"/Conta/{conta.Numero}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        var busca = await client.GetAsync($"/Conta/{conta.Numero}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, busca.StatusCode);
    }

    [Fact]
    public async Task ContaComLancamentosDevolve409()
    {
        using var client = await _app.CriarClienteAutenticadoAsync("joao@teste.com");
        var conta = await _app.CriarContaAsync(client);
        await _app.DepositarAsync(conta.Numero, 10m);

        var resposta = await client.DeleteAsync($"/Conta/{conta.Numero}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
        var busca = await client.GetAsync($"/Conta/{conta.Numero}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, busca.StatusCode);
    }
}
