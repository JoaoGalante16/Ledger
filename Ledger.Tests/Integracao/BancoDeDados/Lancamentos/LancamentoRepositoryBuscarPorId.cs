using Ledger.Infrastructure.Repositories;
using Ledger.Tests.Integracao.BancoDeDados.Suporte;

namespace Ledger.Tests.Integracao.BancoDeDados.Lancamentos;

[Collection(nameof(ContextoCollection))]
public class LancamentoRepositoryBuscarPorId : IAsyncLifetime
{
    private readonly ContextoFixture _fixture;

    public LancamentoRepositoryBuscarPorId(ContextoFixture fixture)
    {
        _fixture = fixture;
    }

    public async ValueTask InitializeAsync() => await _fixture.LimparBancoAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task RetornaLancamentoComAContaQuandoIdExiste()
    {
        var conta = await _fixture.CriarContaAsync(await _fixture.CriarUsuarioAsync());
        var lancamento = await _fixture.CriarLancamentoAsync(conta, 75m);
        await using var contexto = _fixture.CriarContexto();
        var repositorio = new LancamentoRepository(contexto);
        
        var lancamentoEncontrado = await repositorio.BuscarPorId(l => l.Id == lancamento.Id);
        
        Assert.NotNull(lancamentoEncontrado);
        Assert.Equal(75m, lancamentoEncontrado.Valor.Value);
        Assert.Equal(conta.IdUsuario, lancamentoEncontrado.Conta.IdUsuario);
    }

    [Fact]
    public async Task RetornaNuloQuandoIdNaoExiste()
    {
        await using var contexto = _fixture.CriarContexto();
        var repositorio = new LancamentoRepository(contexto);
        
        var lancamentoEncontrado = await repositorio.BuscarPorId(l => l.Id == 999_999);
        
        Assert.Null(lancamentoEncontrado);
    }
}
