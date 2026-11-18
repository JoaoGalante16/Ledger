using Ledger.Infrastructure.Repositories;
using Ledger.Tests.Integracao.BancoDeDados.Suporte;

namespace Ledger.Tests.Integracao.BancoDeDados.Lancamentos;

[Collection(nameof(ContextoCollection))]
public class LancamentoRepositoryBuscarPagina : IAsyncLifetime
{
    private readonly ContextoFixture _fixture;

    public LancamentoRepositoryBuscarPagina(ContextoFixture fixture)
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

        var pagina = await repositorio.BuscarPaginaAsync(1, 20, joao.Id);

        Assert.Equal(2, pagina.TotalItens);
        Assert.All(pagina.Itens, l => Assert.Equal(contaDoJoao.Numero, l.NumeroConta));
    }

    [Fact]
    public async Task RetornaOsMaisNovosPrimeiroComOTotal()
    {
        var conta = await _fixture.CriarContaAsync(await _fixture.CriarUsuarioAsync());
        await _fixture.CriarLancamentoAsync(conta, 10m);
        var segundo = await _fixture.CriarLancamentoAsync(conta, 20m);
        var terceiro = await _fixture.CriarLancamentoAsync(conta, -5m);
        await using var contexto = _fixture.CriarContexto();
        var repositorio = new LancamentoRepository(contexto);

        var pagina = await repositorio.BuscarPaginaAsync(1, 2);

        Assert.Equal(new[] { terceiro.Id, segundo.Id }, pagina.Itens.Select(l => l.Id));
        Assert.Equal(3, pagina.TotalItens);
    }
}
