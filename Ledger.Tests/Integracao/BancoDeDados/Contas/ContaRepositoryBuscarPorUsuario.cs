using Ledger.Infrastructure.Repositories;
using Ledger.Tests.Integracao.BancoDeDados.Suporte;

namespace Ledger.Tests.Integracao.BancoDeDados.Contas;

[Collection(nameof(ContextoCollection))]
public class ContaRepositoryBuscarPorUsuario : IAsyncLifetime
{
    private readonly ContextoFixture _fixture;

    public ContaRepositoryBuscarPorUsuario(ContextoFixture fixture)
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

        var contas = await repositorio.BuscarPorUsuarioAsync(joao.Id);

        Assert.Equal(2, contas.Count);
        Assert.All(contas, c => Assert.Equal(joao.Id, c.IdUsuario));
    }
}
