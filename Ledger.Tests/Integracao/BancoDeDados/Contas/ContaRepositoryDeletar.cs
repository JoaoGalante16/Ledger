using Ledger.Data.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Ledger.Tests.Integracao.BancoDeDados.Suporte;

namespace Ledger.Tests.Integracao.BancoDeDados.Contas;

[Collection(nameof(ContextoCollection))]
public class ContaRepositoryDeletar : IAsyncLifetime
{
    private readonly ContextoFixture _fixture;

    public ContaRepositoryDeletar(ContextoFixture fixture)
    {
        _fixture = fixture;
    }

    public async ValueTask InitializeAsync() => await _fixture.LimparBancoAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task RemoveContaSemLancamentosDoBanco()
    {
        var conta = await _fixture.CriarContaAsync(await _fixture.CriarUsuarioAsync());
        await using var contexto = _fixture.CriarContexto();
        var unitOfWork = new UnitOfWork(contexto);
        var contaParaRemover = await unitOfWork.ContaRepository.BuscarPorPk(c => c.Numero == conta.Numero);
        
        await unitOfWork.ContaRepository.Deletar(contaParaRemover!);
        await unitOfWork.Commit();
        
        await using var leitura = _fixture.CriarContexto();
        Assert.False(await leitura.Contas.AnyAsync(c => c.Numero == conta.Numero, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task BancoImpedeRemoverContaComLancamentos()
    {
        var conta = await _fixture.CriarContaAsync(await _fixture.CriarUsuarioAsync());
        await _fixture.CriarLancamentoAsync(conta, 100m);
        await using var contexto = _fixture.CriarContexto();
        var unitOfWork = new UnitOfWork(contexto);
        var contaParaRemover = await unitOfWork.ContaRepository.BuscarPorPk(c => c.Numero == conta.Numero);
        
        await unitOfWork.ContaRepository.Deletar(contaParaRemover!);

        await Assert.ThrowsAsync<DbUpdateException>(() => unitOfWork.Commit());
        await using var leitura = _fixture.CriarContexto();
        Assert.True(await leitura.Contas.AnyAsync(c => c.Numero == conta.Numero, TestContext.Current.CancellationToken));
    }
}
