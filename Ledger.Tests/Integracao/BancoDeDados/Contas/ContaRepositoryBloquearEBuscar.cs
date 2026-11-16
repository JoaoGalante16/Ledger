using Ledger.Infrastructure.Repositories;
using Ledger.Tests.Integracao.BancoDeDados.Suporte;

namespace Ledger.Tests.Integracao.BancoDeDados.Contas;

[Collection(nameof(ContextoCollection))]
public class ContaRepositoryBloquearEBuscar : IAsyncLifetime
{
    private readonly ContextoFixture _fixture;

    public ContaRepositoryBloquearEBuscar(ContextoFixture fixture)
    {
        _fixture = fixture;
    }

    public async ValueTask InitializeAsync() => await _fixture.LimparBancoAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task RetornaSoAsContasPedidasEmOrdemDeNumero()
    {
        var dono = await _fixture.CriarUsuarioAsync();
        var primeira = await _fixture.CriarContaAsync(dono);
        await _fixture.CriarContaAsync(dono);
        var terceira = await _fixture.CriarContaAsync(dono);
        await using var contexto = _fixture.CriarContexto();
        var repositorio = new ContaRepository(contexto);

        var contas = await repositorio.BloquearEBuscarAsync(terceira.Numero, primeira.Numero);

        Assert.Equal(new[] { primeira.Numero, terceira.Numero }, contas.Select(c => c.Numero));
    }
}
