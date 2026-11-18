using Ledger.Infrastructure.Repositories;
using Ledger.Tests.Integracao.BancoDeDados.Suporte;

namespace Ledger.Tests.Integracao.BancoDeDados.Contas;

[Collection(nameof(ContextoCollection))]
public class ContaRepositoryBuscarPagina : IAsyncLifetime
{
    private readonly ContextoFixture _fixture;

    public ContaRepositoryBuscarPagina(ContextoFixture fixture)
    {
        _fixture = fixture;
    }

    public async ValueTask InitializeAsync() => await _fixture.LimparBancoAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task RetornaSoAsContasDoUsuario()
    {
        var joao = await _fixture.CriarUsuarioAsync();
        var maria = await _fixture.CriarUsuarioAsync();
        await _fixture.CriarContaAsync(joao);
        await _fixture.CriarContaAsync(joao);
        await _fixture.CriarContaAsync(maria);
        await using var contexto = _fixture.CriarContexto();
        var repositorio = new ContaRepository(contexto);

        var pagina = await repositorio.BuscarPaginaAsync(1, 20, joao.Id);

        Assert.Equal(2, pagina.TotalItens);
        Assert.All(pagina.Itens, c => Assert.Equal(joao.Id, c.IdUsuario));
    }

    [Fact]
    public async Task RetornaAPaginaPedidaEmOrdemDeNumeroComOTotal()
    {
        var joao = await _fixture.CriarUsuarioAsync();
        var maria = await _fixture.CriarUsuarioAsync();
        await _fixture.CriarContaAsync(joao);
        await _fixture.CriarContaAsync(maria);
        var terceira = await _fixture.CriarContaAsync(joao);
        await using var contexto = _fixture.CriarContexto();
        var repositorio = new ContaRepository(contexto);

        var pagina = await repositorio.BuscarPaginaAsync(2, 2);

        Assert.Equal(new[] { terceira.Numero }, pagina.Itens.Select(c => c.Numero));
        Assert.Equal(3, pagina.TotalItens);
    }
}
