using Ledger.Data.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Ledger.Tests.Integracao.BancoDeDados.Suporte;

namespace Ledger.Tests.Integracao.BancoDeDados.Contas;

[Collection(nameof(ContextoCollection))]
public class ContaRepositoryAtualizar : IAsyncLifetime
{
    private readonly ContextoFixture _fixture;

    public ContaRepositoryAtualizar(ContextoFixture fixture)
    {
        _fixture = fixture;
    }

    public async ValueTask InitializeAsync() => await _fixture.LimparBancoAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task AtualizaONomeDaContaNoBanco()
    {
        var conta = await _fixture.CriarContaAsync(await _fixture.CriarUsuarioAsync());
        await using var contexto = _fixture.CriarContexto();
        var unitOfWork = new UnitOfWork(contexto);
        var contaParaAlterar = await unitOfWork.ContaRepository.BuscarPorPk(c => c.Numero == conta.Numero);
        contaParaAlterar!.Nome = "Conta da viagem";
        
        await unitOfWork.ContaRepository.Atualizar(contaParaAlterar);
        await unitOfWork.Commit();
        
        await using var leitura = _fixture.CriarContexto();
        var contaSalva = await leitura.Contas.SingleAsync(c => c.Numero == conta.Numero, TestContext.Current.CancellationToken);
        Assert.Equal("Conta da viagem", contaSalva.Nome);
        Assert.Equal(conta.Cpf, contaSalva.Cpf);
    }

    [Fact]
    public async Task GravaDataDeEncerramentoSoComOCommit()
    {
        var conta = await _fixture.CriarContaAsync(await _fixture.CriarUsuarioAsync());
        await using var contexto = _fixture.CriarContexto();
        var unitOfWork = new UnitOfWork(contexto);
        var contaParaEncerrar = await unitOfWork.ContaRepository.BuscarPorPk(c => c.Numero == conta.Numero);
        
        contaParaEncerrar!.DataEncerramento = DateTime.UtcNow;
        await unitOfWork.Commit();
        
        await using var leitura = _fixture.CriarContexto();
        var contaSalva = await leitura.Contas.SingleAsync(c => c.Numero == conta.Numero, TestContext.Current.CancellationToken);
        Assert.NotNull(contaSalva.DataEncerramento);
    }
}
