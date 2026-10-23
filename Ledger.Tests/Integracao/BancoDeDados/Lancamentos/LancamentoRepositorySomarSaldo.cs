using Ledger.Infrastructure.Repositories;
using Ledger.Tests.Integracao.BancoDeDados.Suporte;

namespace Ledger.Tests.Integracao.BancoDeDados.Lancamentos;

[Collection(nameof(ContextoCollection))]
public class LancamentoRepositorySomarSaldo : IAsyncLifetime
{
    private readonly ContextoFixture _fixture;

    public LancamentoRepositorySomarSaldo(ContextoFixture fixture)
    {
        _fixture = fixture;
    }

    public async ValueTask InitializeAsync() => await _fixture.LimparBancoAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task SomaOSaldoDeUmaContaNoBanco()
    {
        var conta = await _fixture.CriarContaAsync(await _fixture.CriarUsuarioAsync());
        var outraConta = await _fixture.CriarContaAsync(await _fixture.CriarUsuarioAsync());
        await _fixture.CriarLancamentoAsync(conta, 0.10m);
        await _fixture.CriarLancamentoAsync(conta, 0.20m);
        await _fixture.CriarLancamentoAsync(outraConta, 1000m);
        await using var contexto = _fixture.CriarContexto();
        var repositorio = new LancamentoRepository(contexto);

        var saldo = await repositorio.SomarSaldo(conta.Numero);

        Assert.Equal(0.30m, saldo);
    }

    [Fact]
    public async Task SomaZeroQuandoAContaNaoTemLancamentos()
    {
        var conta = await _fixture.CriarContaAsync(await _fixture.CriarUsuarioAsync());
        await using var contexto = _fixture.CriarContexto();
        var repositorio = new LancamentoRepository(contexto);

        var saldo = await repositorio.SomarSaldo(conta.Numero);

        Assert.Equal(0m, saldo);
    }
}
