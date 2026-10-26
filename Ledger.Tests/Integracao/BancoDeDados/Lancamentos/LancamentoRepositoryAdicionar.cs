using Ledger.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Ledger.Tests.Integracao.BancoDeDados.Suporte;

namespace Ledger.Tests.Integracao.BancoDeDados.Lancamentos;

[Collection(nameof(ContextoCollection))]
public class LancamentoRepositoryAdicionar : IAsyncLifetime
{
    private readonly ContextoFixture _fixture;

    public LancamentoRepositoryAdicionar(ContextoFixture fixture)
    {
        _fixture = fixture;
    }

    public async ValueTask InitializeAsync() => await _fixture.LimparBancoAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task RegistraLancamentoNoBancoComInformacoesCorretas()
    {
        var conta = await _fixture.CriarContaAsync(await _fixture.CriarUsuarioAsync());
        var lancamento = _fixture.NovoLancamento(conta, 120.55m);
        await using var contexto = _fixture.CriarContexto();
        var unitOfWork = new UnitOfWork(contexto);
        
        await unitOfWork.LancamentoRepository.AdicionarAsync(lancamento);
        await unitOfWork.CommitAsync();
        
        await using var leitura = _fixture.CriarContexto();
        var lancamentoSalvo = await leitura.Lancamentos.SingleAsync(l => l.Id == lancamento.Id, TestContext.Current.CancellationToken);
        Assert.Equal(conta.Numero, lancamentoSalvo.NumeroConta);
        Assert.Equal(120.55m, lancamentoSalvo.Valor.Value);
        Assert.Equal(lancamento.Descricao, lancamentoSalvo.Descricao);
        Assert.Equal(DateTimeKind.Utc, lancamentoSalvo.DataTransacao.Kind);
        Assert.Equal(lancamento.DataTransacao, lancamentoSalvo.DataTransacao, TimeSpan.FromMicroseconds(1));
    }

    [Fact]
    public async Task PreencheIdTransacaoPelaSequenceQuandoNaoInformado()
    {
        var conta = await _fixture.CriarContaAsync(await _fixture.CriarUsuarioAsync());
        var lancamento = _fixture.NovoLancamento(conta, 10m);
        await using var contexto = _fixture.CriarContexto();
        var unitOfWork = new UnitOfWork(contexto);
        
        await unitOfWork.LancamentoRepository.AdicionarAsync(lancamento);
        await unitOfWork.CommitAsync();
        
        Assert.True(lancamento.IdTransacao > 0);
    }

    [Fact]
    public async Task NaoRegistraNenhumaLinhaQuandoUmaDelasFalha()
    {
        var conta = await _fixture.CriarContaAsync(await _fixture.CriarUsuarioAsync());
        var debito = _fixture.NovoLancamento(conta, -50m);
        var credito = _fixture.NovoLancamento(conta, 50m);
        credito.NumeroConta = 999999;
        await using var contexto = _fixture.CriarContexto();
        var unitOfWork = new UnitOfWork(contexto);
        
        await unitOfWork.LancamentoRepository.AdicionarAsync(debito);
        await unitOfWork.LancamentoRepository.AdicionarAsync(credito);
        
        await Assert.ThrowsAsync<DbUpdateException>(() => unitOfWork.CommitAsync());
        await using var leitura = _fixture.CriarContexto();
        Assert.Equal(0, await leitura.Lancamentos.CountAsync(TestContext.Current.CancellationToken));
    }
}
