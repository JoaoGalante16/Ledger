using Ledger.Data.Repositories;
using Ledger.Tests.Integracao.Suporte;

namespace Ledger.Tests.Integracao.Contas;

[Collection(nameof(ContextoCollection))]
public class ContaRepositoryBuscarPorPk : IAsyncLifetime
{
    private readonly ContextoFixture _fixture;

    public ContaRepositoryBuscarPorPk(ContextoFixture fixture)
    {
        _fixture = fixture;
    }

    public async ValueTask InitializeAsync() => await _fixture.LimparBancoAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task RetornaContaQuandoNumeroExiste()
    {
        var conta = await _fixture.CriarContaAsync(await _fixture.CriarUsuarioAsync());
        await using var contexto = _fixture.CriarContexto();
        var repositorio = new ContaRepository(contexto);
        
        var contaEncontrada = await repositorio.BuscarPorPk(c => c.Numero == conta.Numero);
        
        Assert.NotNull(contaEncontrada);
        Assert.Equal(conta.Nome, contaEncontrada.Nome);
    }

    [Fact]
    public async Task RetornaNuloQuandoNumeroNaoExiste()
    {
        await using var contexto = _fixture.CriarContexto();
        var repositorio = new ContaRepository(contexto);
        
        var contaEncontrada = await repositorio.BuscarPorPk(c => c.Numero == 999_999);
        
        Assert.Null(contaEncontrada);
    }

    [Fact]
    public async Task RetornaNuloQuandoContaEhDeOutroUsuario()
    {
        var conta = await _fixture.CriarContaAsync(await _fixture.CriarUsuarioAsync());
        var outroUsuario = await _fixture.CriarUsuarioAsync();
        await using var contexto = _fixture.CriarContexto();
        var repositorio = new ContaRepository(contexto);
        
        var contaEncontrada = await repositorio.BuscarPorPk(c => c.Numero == conta.Numero && c.IdUsuario == outroUsuario.Id);
        
        Assert.Null(contaEncontrada);
    }
}
