using Ledger.Domain.Constantes;
using Ledger.Infrastructure.Data.Seeds;
using Ledger.Infrastructure.Repositories;
using Ledger.Tests.Integracao.BancoDeDados.Suporte;

namespace Ledger.Tests.Integracao.BancoDeDados.Contas;

[Collection(nameof(ContextoCollection))]
public class ContaRepositoryBuscarContaCaixa : IAsyncLifetime
{
    private readonly ContextoFixture _fixture;

    public ContaRepositoryBuscarContaCaixa(ContextoFixture fixture)
    {
        _fixture = fixture;
    }

    public async ValueTask InitializeAsync() => await _fixture.LimparBancoAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task RetornaAContaDoUsuarioDeSistemaCriadaPeloSeed()
    {
        await using (var contextoDoSeed = _fixture.CriarContexto())
        {
            await new SeedContaCaixa(contextoDoSeed).CriarAsync();
        }
        await _fixture.CriarContaAsync(await _fixture.CriarUsuarioAsync());
        await using var contexto = _fixture.CriarContexto();
        var repositorio = new ContaRepository(contexto);

        var caixa = await repositorio.BuscarContaCaixaAsync();

        Assert.Equal(ContaCaixa.IdUsuario, caixa.IdUsuario);
        Assert.Equal(ContaCaixa.Nome, caixa.Nome);
        Assert.Equal(ContaCaixa.Cpf, caixa.Cpf.Value);
    }

    [Fact]
    public async Task SeedNaoDuplicaACaixaQuandoRodaDuasVezes()
    {
        await using (var contextoDoSeed = _fixture.CriarContexto())
        {
            await new SeedContaCaixa(contextoDoSeed).CriarAsync();
            await new SeedContaCaixa(contextoDoSeed).CriarAsync();
        }
        await using var contexto = _fixture.CriarContexto();
        var repositorio = new ContaRepository(contexto);

        var caixa = await repositorio.BuscarContaCaixaAsync();

        Assert.Equal(ContaCaixa.IdUsuario, caixa.IdUsuario);
    }
}
