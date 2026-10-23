using System.Net;
using System.Net.Http.Json;
using Ledger.Application.Dtos.Lancamentos;
using Ledger.Tests.Integracao.Api.Suporte;

namespace Ledger.Tests.Integracao.Api.Lancamentos;

[Collection(nameof(ApiCollection))]
public class LancamentoTransacao : IAsyncLifetime
{
    private readonly LedgerWebApplicationFactory _app;

    public LancamentoTransacao(LedgerWebApplicationFactory app)
    {
        _app = app;
    }

    public async ValueTask InitializeAsync() => await _app.LimparBancoAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private async Task<int> TransferirAsync(HttpClient client, decimal valor)
    {
        var origem = await _app.CriarContaAsync(client);
        var destino = await _app.CriarContaAsync(client, "Poupança");
        await _app.DepositarAsync(origem.Numero, valor);

        var resposta = await client.PostAsJsonAsync("/Lancamento",
            new { numeroContaOrigem = origem.Numero, numeroContaDestino = destino.Numero, valor },
            TestContext.Current.CancellationToken);
        var par = await resposta.Content.ReadFromJsonAsync<List<ReadLancamentoDto>>(TestContext.Current.CancellationToken);
        return par![0].IdTransacao;
    }

    [Fact]
    public async Task AdminBuscaOParDaTransacao()
    {
        using var joao = await _app.CriarClienteAutenticadoAsync("joao@teste.com");
        using var admin = await _app.CriarClienteAdminAsync("admin@teste.com");
        var idTransacao = await TransferirAsync(joao, 60m);

        var par = await admin.GetFromJsonAsync<List<ReadLancamentoDto>>($"/Lancamento/transacao/{idTransacao}", TestContext.Current.CancellationToken);

        Assert.NotNull(par);
        Assert.Equal(new[] { -60m, 60m }, par.Select(l => l.Valor));
    }

    [Fact]
    public async Task UsuarioComumDevolve403()
    {
        using var joao = await _app.CriarClienteAutenticadoAsync("joao@teste.com");
        var idTransacao = await TransferirAsync(joao, 60m);

        var resposta = await joao.GetAsync($"/Lancamento/transacao/{idTransacao}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }
}
