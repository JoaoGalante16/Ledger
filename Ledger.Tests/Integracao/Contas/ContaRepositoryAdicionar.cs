using Ledger.Data.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Ledger.Tests.Integracao.Suporte;

namespace Ledger.Tests.Integracao.Contas;

[Collection(nameof(ContextoCollection))]
public class ContaRepositoryAdicionar : IAsyncLifetime
{
    private readonly ContextoFixture _fixture;

    public ContaRepositoryAdicionar(ContextoFixture fixture)
    {
        _fixture = fixture;
    }

    public async ValueTask InitializeAsync() => await _fixture.LimparBancoAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task RegistraContaNoBancoComInformacoesCorretas()
    {
        var dono = await _fixture.CriarUsuarioAsync();
        var conta = _fixture.NovaConta(dono);
        await using var contexto = _fixture.CriarContexto();
        var unitOfWork = new UnitOfWork(contexto);
        
        await unitOfWork.ContaRepository.Adicionar(conta);
        await unitOfWork.Commit();
        
        await using var leitura = _fixture.CriarContexto();
        var contaSalva = await leitura.Contas.SingleAsync(c => c.Numero == conta.Numero, TestContext.Current.CancellationToken);
        Assert.True(conta.Numero > 0);
        Assert.Equal(conta.Nome, contaSalva.Nome);
        Assert.Equal(conta.Cpf, contaSalva.Cpf);
        Assert.Equal(dono.Id, contaSalva.IdUsuario);
        Assert.Null(contaSalva.DataEncerramento);
    }

    [Fact]
    public async Task NaoRegistraContaSemCommit()
    {
        var dono = await _fixture.CriarUsuarioAsync();
        var conta = _fixture.NovaConta(dono);
        await using var contexto = _fixture.CriarContexto();
        var unitOfWork = new UnitOfWork(contexto);
        
        await unitOfWork.ContaRepository.Adicionar(conta);
        
        await using var leitura = _fixture.CriarContexto();
        Assert.Equal(0, await leitura.Contas.CountAsync(TestContext.Current.CancellationToken));
    }
}
