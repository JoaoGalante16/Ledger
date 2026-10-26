using Ledger.Infrastructure.Repositories;
using Ledger.Tests.Integracao.BancoDeDados.Suporte;

namespace Ledger.Tests.Integracao.BancoDeDados.Lancamentos;

[Collection(nameof(ContextoCollection))]
public class LancamentoRepositoryExisteLancamentoNaConta : IAsyncLifetime
{
    private readonly ContextoFixture _fixture;

    public LancamentoRepositoryExisteLancamentoNaConta(ContextoFixture fixture)
    {
        _fixture = fixture;
    }

    public async ValueTask InitializeAsync() => await _fixture.LimparBancoAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task RetornaVerdadeiroQuandoAContaTemLancamento()
    {
        var conta = await _fixture.CriarContaAsync(await _fixture.CriarUsuarioAsync());
        await _fixture.CriarLancamentoAsync(conta, 10m);
        await using var contexto = _fixture.CriarContexto();
        var repositorio = new LancamentoRepository(contexto);

        var existe = await repositorio.ExisteLancamentoNaContaAsync(conta.Numero);

        Assert.True(existe);
    }

    [Fact]
    public async Task RetornaFalsoQuandoAContaNaoTemLancamento()
    {
        var conta = await _fixture.CriarContaAsync(await _fixture.CriarUsuarioAsync());
        var outraConta = await _fixture.CriarContaAsync(await _fixture.CriarUsuarioAsync());
        await _fixture.CriarLancamentoAsync(outraConta, 10m);
        await using var contexto = _fixture.CriarContexto();
        var repositorio = new LancamentoRepository(contexto);

        var existe = await repositorio.ExisteLancamentoNaContaAsync(conta.Numero);

        Assert.False(existe);
    }
}
