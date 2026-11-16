using System.Net;
using System.Net.Http.Json;
using Ledger.Tests.Integracao.Api.Suporte;
using Microsoft.EntityFrameworkCore;

namespace Ledger.Tests.Integracao.Api.Lancamentos;

[Collection(nameof(ApiCollection))]
public class LancamentoConcorrencia : IAsyncLifetime
{
    private const int QuantidadeDeSaques = 10;

    private readonly LedgerWebApplicationFactory _app;

    public LancamentoConcorrencia(LedgerWebApplicationFactory app)
    {
        _app = app;
    }

    public async ValueTask InitializeAsync() => await _app.LimparBancoAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task SaquesSimultaneosNaoDeixamSaldoNegativo()
    {
        var cancelamento = TestContext.Current.CancellationToken;
        using var joao = await _app.CriarClienteAutenticadoAsync("joao@teste.com");
        var conta = await _app.CriarContaAsync(joao);
        await _app.DepositarAsync(conta.Numero, 100m);

        var saques = Enumerable.Range(0, QuantidadeDeSaques)
            .Select(_ => joao.PostAsJsonAsync("/Lancamento/saque", new { numeroConta = conta.Numero, valor = 100m }, cancelamento));
        var respostas = await Task.WhenAll(saques);

        Assert.Single(respostas, r => r.StatusCode == HttpStatusCode.OK);
        Assert.Equal(QuantidadeDeSaques - 1, respostas.Count(r => r.StatusCode == HttpStatusCode.BadRequest));

        await using var contexto = _app.CriarContexto();
        var contaNoBanco = await contexto.Contas.SingleAsync(c => c.Numero == conta.Numero, cancelamento);
        var somaDosLancamentos = await contexto.Lancamentos
            .Where(l => l.NumeroConta == conta.Numero)
            .SumAsync(l => l.Valor.Value, cancelamento);
        Assert.Equal(0m, contaNoBanco.Saldo.Value);
        Assert.Equal(contaNoBanco.Saldo.Value, somaDosLancamentos);
    }

    [Fact]
    public async Task TransferenciasCruzadasSimultaneasMantemOsSaldos()
    {
        var cancelamento = TestContext.Current.CancellationToken;
        using var joao = await _app.CriarClienteAutenticadoAsync("joao@teste.com");
        using var maria = await _app.CriarClienteAutenticadoAsync("maria@teste.com");
        var contaJoao = await _app.CriarContaAsync(joao);
        var contaMaria = await _app.CriarContaAsync(maria);
        await _app.DepositarAsync(contaJoao.Numero, 1000m);
        await _app.DepositarAsync(contaMaria.Numero, 1000m);

        var transferencias = Enumerable.Range(0, QuantidadeDeSaques).SelectMany(_ => new[]
        {
            joao.PostAsJsonAsync("/Lancamento", new { numeroContaOrigem = contaJoao.Numero, numeroContaDestino = contaMaria.Numero, valor = 10m, descricao = "Pix" }, cancelamento),
            maria.PostAsJsonAsync("/Lancamento", new { numeroContaOrigem = contaMaria.Numero, numeroContaDestino = contaJoao.Numero, valor = 10m, descricao = "Pix" }, cancelamento)
        });
        var respostas = await Task.WhenAll(transferencias);

        Assert.All(respostas, r => Assert.Equal(HttpStatusCode.OK, r.StatusCode));

        await using var contexto = _app.CriarContexto();
        var saldos = await contexto.Contas
            .Where(c => c.Numero == contaJoao.Numero || c.Numero == contaMaria.Numero)
            .Select(c => c.Saldo.Value)
            .ToListAsync(cancelamento);
        Assert.All(saldos, saldo => Assert.Equal(1000m, saldo));
    }
}
