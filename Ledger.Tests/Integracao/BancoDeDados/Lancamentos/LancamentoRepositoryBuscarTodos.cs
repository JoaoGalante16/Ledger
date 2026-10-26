using Ledger.Infrastructure.Repositories;
using Ledger.Tests.Integracao.BancoDeDados.Suporte;

namespace Ledger.Tests.Integracao.BancoDeDados.Lancamentos;

[Collection(nameof(ContextoCollection))]
public class LancamentoRepositoryBuscarTodos : IAsyncLifetime
{
    private readonly ContextoFixture _fixture;

    public LancamentoRepositoryBuscarTodos(ContextoFixture fixture)
    {
        _fixture = fixture;
    }

    public async ValueTask InitializeAsync() => await _fixture.LimparBancoAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task RetornaTodosOsLancamentos()
    {
        var conta = await _fixture.CriarContaAsync(await _fixture.CriarUsuarioAsync());
        await _fixture.CriarLancamentoAsync(conta, 10m);
        await _fixture.CriarLancamentoAsync(conta, 20m);
        await _fixture.CriarLancamentoAsync(conta, -5m);
        await using var contexto = _fixture.CriarContexto();
        var repositorio = new LancamentoRepository(contexto);

        var lancamentos = await repositorio.BuscarTodosAsync();

        Assert.Equal(3, lancamentos.Count);
    }
}
