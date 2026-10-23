using Ledger.Infrastructure.Repositories;
using Ledger.Tests.Integracao.BancoDeDados.Suporte;

namespace Ledger.Tests.Integracao.BancoDeDados.Lancamentos;

[Collection(nameof(ContextoCollection))]
public class LancamentoRepositoryBuscarPorUsuario : IAsyncLifetime
{
    private readonly ContextoFixture _fixture;

    public LancamentoRepositoryBuscarPorUsuario(ContextoFixture fixture)
    {
        _fixture = fixture;
    }

    public async ValueTask InitializeAsync() => await _fixture.LimparBancoAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task RetornaSoOsLancamentosDasContasDoUsuario()
    {
        var joao = await _fixture.CriarUsuarioAsync();
        var maria = await _fixture.CriarUsuarioAsync();
        var contaDoJoao = await _fixture.CriarContaAsync(joao);
        var contaDaMaria = await _fixture.CriarContaAsync(maria);
        await _fixture.CriarLancamentoAsync(contaDoJoao, 10m);
        await _fixture.CriarLancamentoAsync(contaDoJoao, 20m);
        await _fixture.CriarLancamentoAsync(contaDaMaria, 30m);
        await using var contexto = _fixture.CriarContexto();
        var repositorio = new LancamentoRepository(contexto);

        var lancamentos = await repositorio.BuscarPorUsuario(joao.Id);

        Assert.Equal(2, lancamentos.Count);
        Assert.All(lancamentos, l => Assert.Equal(contaDoJoao.Numero, l.NumeroConta));
    }
}
