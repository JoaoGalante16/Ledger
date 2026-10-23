using Ledger.Infrastructure.Repositories;
using Ledger.Tests.Integracao.BancoDeDados.Suporte;

namespace Ledger.Tests.Integracao.BancoDeDados.Lancamentos;

[Collection(nameof(ContextoCollection))]
public class LancamentoRepositorySomarSaldos : IAsyncLifetime
{
    private readonly ContextoFixture _fixture;

    public LancamentoRepositorySomarSaldos(ContextoFixture fixture)
    {
        _fixture = fixture;
    }

    public async ValueTask InitializeAsync() => await _fixture.LimparBancoAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task SomaOSaldoDeCadaContaPedida()
    {
        var usuario = await _fixture.CriarUsuarioAsync();
        var conta = await _fixture.CriarContaAsync(usuario);
        var poupanca = await _fixture.CriarContaAsync(usuario);
        var contaDeFora = await _fixture.CriarContaAsync(usuario);
        await _fixture.CriarLancamentoAsync(conta, 500m);
        await _fixture.CriarLancamentoAsync(conta, -200m);
        await _fixture.CriarLancamentoAsync(poupanca, 50m);
        await _fixture.CriarLancamentoAsync(contaDeFora, 1000m);
        await using var contexto = _fixture.CriarContexto();
        var repositorio = new LancamentoRepository(contexto);

        var saldos = await repositorio.SomarSaldos([conta.Numero, poupanca.Numero]);

        Assert.Equal(2, saldos.Count);
        Assert.Equal(300m, saldos[conta.Numero]);
        Assert.Equal(50m, saldos[poupanca.Numero]);
    }

    [Fact]
    public async Task ContaSemLancamentosNaoApareceNoResultado()
    {
        var conta = await _fixture.CriarContaAsync(await _fixture.CriarUsuarioAsync());
        await using var contexto = _fixture.CriarContexto();
        var repositorio = new LancamentoRepository(contexto);

        var saldos = await repositorio.SomarSaldos([conta.Numero]);

        Assert.Empty(saldos);
    }
}
