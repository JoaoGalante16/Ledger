using Ledger.Application.Dtos.Lancamentos;
using Ledger.Application.Errors;
using Ledger.Application.Services;
using Ledger.Domain.Entities;
using Ledger.Tests.Unitarios.Suporte;
using Moq;

namespace Ledger.Tests.Unitarios.Services;

public class LancamentoServiceCriar
{
    private readonly CenarioDeServico _cenario = new();
    private readonly LancamentoService _service;

    public LancamentoServiceCriar()
    {
        _service = new LancamentoService(_cenario.UnitOfWork.Object, _cenario.Mapper);
    }

    private static CreateLancamentoDto Transferencia(int origem, int destino, decimal valor) => new()
    {
        NumeroContaOrigem = origem,
        NumeroContaDestino = destino,
        Valor = valor,
        Descricao = "Aluguel"
    };

    // ---- Caminho feliz ----

    [Fact]
    public async Task GravaParDeLancamentosComMesmoIdTransacaoQuandoTransferenciaValida()
    {
        var origem = _cenario.NovaConta(1);
        var destino = _cenario.NovaConta(2, CenarioDeServico.IdOutroUsuario);
        _cenario.DarSaldo(origem, 500m);
        var antes = DateTime.UtcNow;

        var resultado = await _service.Criar(Transferencia(1, 2, 120.50m), CenarioDeServico.IdDono, eAdmin: false);
        var depois = DateTime.UtcNow;

        Assert.True(resultado.IsSuccess);
        Assert.Equal(2, _cenario.LancamentosAdicionados.Count);

        var debito = _cenario.LancamentosAdicionados.Single(l => l.NumeroConta == origem.Numero);
        var credito = _cenario.LancamentosAdicionados.Single(l => l.NumeroConta == destino.Numero);

        Assert.Equal(-120.50m, debito.Valor.Value);
        Assert.Equal(120.50m, credito.Valor.Value);
        Assert.Equal(0m, debito.Valor.Value + credito.Valor.Value);

        Assert.Equal(CenarioDeServico.ProximoIdTransacao, debito.IdTransacao);
        Assert.Equal(CenarioDeServico.ProximoIdTransacao, credito.IdTransacao);
        Assert.All(_cenario.LancamentosAdicionados, l => Assert.Equal("Aluguel", l.Descricao));
        Assert.All(_cenario.LancamentosAdicionados, l =>
        {
            Assert.InRange(l.DataTransacao, antes, depois);
            Assert.InRange(l.DataGravacao, antes, depois);
        });

        _cenario.LancamentoRepository.Verify(r => r.ProximoIdTransacao(), Times.Once);
        _cenario.LancamentoRepository.Verify(r => r.Adicionar(It.IsAny<Lancamento>()), Times.Exactly(2));
        _cenario.UnitOfWork.Verify(u => u.Commit(), Times.Once);

        Assert.Equal(new[] { -120.50m, 120.50m }, resultado.Value.Select(l => l.Valor));
    }

    // ---- Acesso às contas ----

    [Theory]
    [InlineData(99)] // conta que não existe
    [InlineData(3)] // conta que existe, mas é de outro usuário
    public async Task RetornaNaoEncontradoQuandoContaDeOrigemNaoEhAcessivel(int numeroOrigem)
    {
        _cenario.NovaConta(2);
        var contaDeOutro = _cenario.NovaConta(3, CenarioDeServico.IdOutroUsuario);
        _cenario.DarSaldo(contaDeOutro, 1000m);

        var resultado =
            await _service.Criar(Transferencia(numeroOrigem, 2, 10m), CenarioDeServico.IdDono, eAdmin: false);

        Assert.True(resultado.HasError<NaoEncontradoError>());
        Assert.Equal("Conta de origem não encontrada", resultado.Errors[0].Message);
        _cenario.VerificarQueNadaFoiGravado();
    }

    [Fact]
    public async Task AdminTransfereDeContaDeOutroUsuario()
    {
        var origemDeOutro = _cenario.NovaConta(1, CenarioDeServico.IdOutroUsuario);
        _cenario.NovaConta(2);
        _cenario.DarSaldo(origemDeOutro, 100m);

        var resultado = await _service.Criar(Transferencia(1, 2, 50m), "id-do-admin", eAdmin: true);

        Assert.True(resultado.IsSuccess);
        _cenario.UnitOfWork.Verify(u => u.Commit(), Times.Once);
    }

    [Fact]
    public async Task RetornaNaoEncontradoQuandoContaDeDestinoNaoExiste()
    {
        var origem = _cenario.NovaConta(1);
        _cenario.DarSaldo(origem, 100m);

        var resultado = await _service.Criar(Transferencia(1, 99, 10m), CenarioDeServico.IdDono, eAdmin: false);

        Assert.True(resultado.HasError<NaoEncontradoError>());
        Assert.Equal("Conta de destino não encontrada", resultado.Errors[0].Message);
        _cenario.VerificarQueNadaFoiGravado();
    }

    // ---- Contas encerradas ----

    [Theory]
    [InlineData(true, false, "A conta de origem foi encerrada")]
    [InlineData(false, true, "A conta de destino foi encerrada")]
    public async Task RetornaConflitoQuandoAlgumaContaEstaEncerrada(bool origemEncerrada, bool destinoEncerrado,
        string mensagem)
    {
        var origem = _cenario.NovaConta(1, encerrada: origemEncerrada);
        _cenario.NovaConta(2, encerrada: destinoEncerrado);
        _cenario.DarSaldo(origem, 100m);

        var resultado = await _service.Criar(Transferencia(1, 2, 10m), CenarioDeServico.IdDono, eAdmin: false);

        Assert.True(resultado.HasError<ConflitoError>());
        Assert.Equal(mensagem, resultado.Errors[0].Message);
        _cenario.VerificarQueNadaFoiGravado();
    }

    // ---- Saldo ----

    [Fact]
    public async Task RetornaSaldoInsuficienteQuandoSaldoMenorQueValor()
    {
        var origem = _cenario.NovaConta(1);
        _cenario.NovaConta(2);
        _cenario.DarSaldo(origem, 99.99m);

        var resultado = await _service.Criar(Transferencia(1, 2, 100m), CenarioDeServico.IdDono, eAdmin: false);

        Assert.True(resultado.HasError<SaldoInsuficienteError>());
        _cenario.VerificarQueNadaFoiGravado();
        _cenario.LancamentoRepository.Verify(r => r.ProximoIdTransacao(), Times.Never);
    }

    [Fact]
    public async Task PermiteTransferenciaQuandoSaldoIgualAoValor()
    {
        var origem = _cenario.NovaConta(1);
        _cenario.NovaConta(2);
        _cenario.DarSaldo(origem, 100m);

        var resultado = await _service.Criar(Transferencia(1, 2, 100m), CenarioDeServico.IdDono, eAdmin: false);

        Assert.True(resultado.IsSuccess);
        _cenario.UnitOfWork.Verify(u => u.Commit(), Times.Once);
    }
}