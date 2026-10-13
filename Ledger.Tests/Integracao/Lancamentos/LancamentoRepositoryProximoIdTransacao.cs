using Ledger.Data.Repositories;
using Ledger.Tests.Integracao.Suporte;

namespace Ledger.Tests.Integracao.Lancamentos;

[Collection(nameof(ContextoCollection))]
public class LancamentoRepositoryProximoIdTransacao
{
    private readonly ContextoFixture _fixture;

    public LancamentoRepositoryProximoIdTransacao(ContextoFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task RetornaValoresDiferentesECrescentes()
    {
        await using var contexto = _fixture.CriarContexto();
        var repositorio = new LancamentoRepository(contexto);
        
        var primeiro = await repositorio.ProximoIdTransacao();
        var segundo = await repositorio.ProximoIdTransacao();
        
        Assert.True(segundo > primeiro);
    }
}
