using System.Net;
using System.Net.Http.Json;
using Ledger.Application.Dtos.Contas;
using Ledger.Application.Dtos.Lancamentos;
using Ledger.Tests.Integracao.Api.Suporte;

namespace Ledger.Tests.Integracao.Api.Lancamentos;

[Collection(nameof(ApiCollection))]
public class LancamentoCorrecao : IAsyncLifetime
{
    private readonly LedgerWebApplicationFactory _app;

    public LancamentoCorrecao(LedgerWebApplicationFactory app)
    {
        _app = app;
    }

    public async ValueTask InitializeAsync() => await _app.LimparBancoAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private static async Task<int> TransferirAsync(HttpClient client, int origem, int destino, decimal valor)
    {
        var resposta = await client.PostAsJsonAsync("/Lancamento",
            new { numeroContaOrigem = origem, numeroContaDestino = destino, valor },
            TestContext.Current.CancellationToken);
        var par = await resposta.Content.ReadFromJsonAsync<List<ReadLancamentoDto>>(TestContext.Current.CancellationToken);
        return par![0].IdTransacao;
    }

    [Fact]
    public async Task AdminCorrigeATransferenciaEOSaldoVolta()
    {
        using var joao = await _app.CriarClienteAutenticadoAsync("joao@teste.com");
        using var maria = await _app.CriarClienteAutenticadoAsync("maria@teste.com");
        using var admin = await _app.CriarClienteAdminAsync("admin@teste.com");
        var origem = await _app.CriarContaAsync(joao);
        var destino = await _app.CriarContaAsync(maria);
        await _app.DepositarAsync(origem.Numero, 500m);
        var idTransacao = await TransferirAsync(joao, origem.Numero, destino.Numero, 100m);

        var resposta = await admin.PostAsJsonAsync("/Lancamento/correcao",
            new { valor = 100m, descricao = "Estorno", idLancamentoReferencia = idTransacao },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var contaOrigem = await admin.GetFromJsonAsync<ReadContaDto>($"/Conta/{origem.Numero}", TestContext.Current.CancellationToken);
        var contaDestino = await admin.GetFromJsonAsync<ReadContaDto>($"/Conta/{destino.Numero}", TestContext.Current.CancellationToken);
        Assert.Equal(500m, contaOrigem!.Saldo);
        Assert.Equal(0m, contaDestino!.Saldo);
    }

    [Fact]
    public async Task UsuarioComumDevolve403()
    {
        using var joao = await _app.CriarClienteAutenticadoAsync("joao@teste.com");
        var origem = await _app.CriarContaAsync(joao);
        var destino = await _app.CriarContaAsync(joao, "Poupança");
        await _app.DepositarAsync(origem.Numero, 500m);
        var idTransacao = await TransferirAsync(joao, origem.Numero, destino.Numero, 100m);

        var resposta = await joao.PostAsJsonAsync("/Lancamento/correcao",
            new { valor = 100m, descricao = "Estorno", idLancamentoReferencia = idTransacao },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }
}
