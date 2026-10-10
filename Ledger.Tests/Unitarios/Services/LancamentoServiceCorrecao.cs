using Ledger.Data.Dtos.LancamentoDtos;
using Ledger.Results;
using Ledger.Services;
using Ledger.Tests.Unitarios.Suporte;
using Moq;

namespace Ledger.Tests.Unitarios.Services;

public class LancamentoServiceCorrecao
{
    private const int IdTransacaoOriginal = 7;

    private readonly CenarioDeServico _cenario = new();
    private readonly LancamentoService _service;

    public LancamentoServiceCorrecao()
    {
        _service = new LancamentoService(_cenario.UnitOfWork.Object, _cenario.Mapper);
    }

    private static CreateLancamentoCorrecaoDto Correcao(decimal valor) => new()
    {
        Valor = valor,
        Descricao = "Estorno",
        IdLancamentoReferencia = IdTransacaoOriginal
    };

    [Fact]
    public async Task GravaParComSinaisInvertidosApontandoParaAsLinhasOriginais()
    {
        var origem = _cenario.NovaConta(1);
        var destino = _cenario.NovaConta(2);
        var debitoOriginal = _cenario.NovoLancamento(origem, -100m, IdTransacaoOriginal, id: 10);
        var creditoOriginal = _cenario.NovoLancamento(destino, 100m, IdTransacaoOriginal, id: 11);
        
        var resultado = await _service.CriarLancamentoCorrecao(Correcao(100m));
        
        Assert.True(resultado.IsSuccess);
        Assert.Equal(2, _cenario.LancamentosAdicionados.Count);

        var correcaoDaOrigem = _cenario.LancamentosAdicionados.Single(l => l.NumeroConta == origem.Numero);
        var correcaoDoDestino = _cenario.LancamentosAdicionados.Single(l => l.NumeroConta == destino.Numero);

        Assert.Equal(100m, correcaoDaOrigem.Valor);
        Assert.Equal(-100m, correcaoDoDestino.Valor);
        Assert.Equal(debitoOriginal.Id, correcaoDaOrigem.IdLancamentoReferencia);
        Assert.Equal(creditoOriginal.Id, correcaoDoDestino.IdLancamentoReferencia);

        Assert.All(_cenario.LancamentosAdicionados, l =>
        {
            Assert.Equal(CenarioDeServico.ProximoIdTransacao, l.IdTransacao);
            Assert.NotEqual(IdTransacaoOriginal, l.IdTransacao);
        });
        _cenario.UnitOfWork.Verify(u => u.Commit(), Times.Once);
    }

    // ---- Transação original ----

    [Fact]
    public async Task RetornaNaoEncontradoQuandoTransacaoOriginalNaoExiste()
    {
        _cenario.NovaConta(1);

        var resultado = await _service.CriarLancamentoCorrecao(Correcao(100m));

        Assert.True(resultado.HasError<NaoEncontradoError>());
        _cenario.VerificarQueNadaFoiGravado();
    }

    // ---- Contas encerradas ----

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task RetornaConflitoQuandoAlgumaContaDoParEstaEncerrada(bool origemEncerrada, bool destinoEncerrado)
    {
        var origem = _cenario.NovaConta(1, encerrada: origemEncerrada);
        var destino = _cenario.NovaConta(2, encerrada: destinoEncerrado);
        _cenario.NovoLancamento(origem, -100m, IdTransacaoOriginal);
        _cenario.NovoLancamento(destino, 100m, IdTransacaoOriginal);

        var resultado = await _service.CriarLancamentoCorrecao(Correcao(100m));

        Assert.True(resultado.HasError<ConflitoError>());
        _cenario.VerificarQueNadaFoiGravado();
    }
}
