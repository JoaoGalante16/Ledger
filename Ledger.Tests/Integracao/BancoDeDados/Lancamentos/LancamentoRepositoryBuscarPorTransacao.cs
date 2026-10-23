using Ledger.Infrastructure.Repositories;
using Ledger.Tests.Integracao.BancoDeDados.Suporte;

namespace Ledger.Tests.Integracao.BancoDeDados.Lancamentos;

[Collection(nameof(ContextoCollection))]
public class LancamentoRepositoryBuscarPorTransacao : IAsyncLifetime
{
    private readonly ContextoFixture _fixture;

    public LancamentoRepositoryBuscarPorTransacao(ContextoFixture fixture)
    {
        _fixture = fixture;
    }

    public async ValueTask InitializeAsync() => await _fixture.LimparBancoAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task RetornaOParDaTransacaoComODebitoPrimeiro()
    {
        var usuario = await _fixture.CriarUsuarioAsync();
        var origem = await _fixture.CriarContaAsync(usuario);
        var destino = await _fixture.CriarContaAsync(usuario);
        await _fixture.CriarLancamentoAsync(destino, 100m, idTransacao: 500);
        await _fixture.CriarLancamentoAsync(origem, -100m, idTransacao: 500);
        await _fixture.CriarLancamentoAsync(origem, 30m, idTransacao: 501);
        await using var contexto = _fixture.CriarContexto();
        var repositorio = new LancamentoRepository(contexto);

        var par = await repositorio.BuscarPorTransacao(500);

        Assert.Equal(2, par.Count);
        Assert.Equal(-100m, par[0].Valor);
        Assert.Equal(origem.Numero, par[0].NumeroConta);
        Assert.Equal(100m, par[1].Valor);
    }
}
