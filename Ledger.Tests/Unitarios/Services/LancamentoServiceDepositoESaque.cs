using FluentResults;
using Ledger.Application.Dtos.Lancamentos;
using Ledger.Application.Errors;
using Ledger.Application.Services;
using Ledger.Domain.Entities;
using Ledger.Domain.Enums;
using Ledger.Tests.Unitarios.Suporte;
using Moq;

namespace Ledger.Tests.Unitarios.Services;

public class LancamentoServiceDepositoESaque
{
    private readonly CenarioDeServico _cenario = new();
    private readonly LancamentoService _service;

    public LancamentoServiceDepositoESaque()
    {
        _service = new LancamentoService(_cenario.UnitOfWork.Object, _cenario.Mapper);
    }

    private static CreateLancamentoDepositoOuSaque Operacao(int numeroConta, decimal valor) => new()
    {
        NumeroConta = numeroConta,
        Valor = valor
    };

    private Task<Result<List<ReadLancamentoDto>>> ExecutarAsync(string operacao, int numeroConta, decimal valor,
        string idUsuario = CenarioDeServico.IdDono, bool eAdmin = false) =>
        operacao == "deposito"
            ? _service.DepositarAsync(Operacao(numeroConta, valor), idUsuario, eAdmin)
            : _service.SacarAsync(Operacao(numeroConta, valor), idUsuario, eAdmin);

    // ---- Depósito ----

    [Fact]
    public async Task DepositoTiraDaCaixaEColocaNaContaComTipoDeposito()
    {
        var conta = _cenario.NovaConta(1);

        var resultado = await _service.DepositarAsync(Operacao(1, 200m), CenarioDeServico.IdDono, eAdmin: false);

        Assert.True(resultado.IsSuccess);
        var daCaixa = _cenario.LancamentosAdicionados.Single(l => l.NumeroConta == _cenario.Caixa.Numero);
        var naConta = _cenario.LancamentosAdicionados.Single(l => l.NumeroConta == conta.Numero);
        Assert.Equal(-200m, daCaixa.Valor.Value);
        Assert.Equal(200m, naConta.Valor.Value);
        Assert.All(_cenario.LancamentosAdicionados, l =>
        {
            Assert.Equal(TipoOperacao.Deposito, l.TipoOperacao);
            Assert.Equal(CenarioDeServico.ProximoIdTransacao, l.IdTransacao);
        });
        _cenario.UnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task DepositoNaoDependeDoSaldoDaConta()
    {
        _cenario.NovaConta(1);

        var resultado = await _service.DepositarAsync(Operacao(1, 1000m), CenarioDeServico.IdDono, eAdmin: false);

        Assert.True(resultado.IsSuccess);
    }

    // ---- Saque ----

    [Fact]
    public async Task SaqueTiraDaContaEColocaNaCaixaComTipoSaque()
    {
        var conta = _cenario.NovaConta(1);
        _cenario.DarSaldo(conta, 300m);

        var resultado = await _service.SacarAsync(Operacao(1, 120.50m), CenarioDeServico.IdDono, eAdmin: false);

        Assert.True(resultado.IsSuccess);
        var daConta = _cenario.LancamentosAdicionados.Single(l => l.NumeroConta == conta.Numero);
        var naCaixa = _cenario.LancamentosAdicionados.Single(l => l.NumeroConta == _cenario.Caixa.Numero);
        Assert.Equal(-120.50m, daConta.Valor.Value);
        Assert.Equal(120.50m, naCaixa.Valor.Value);
        Assert.All(_cenario.LancamentosAdicionados, l => Assert.Equal(TipoOperacao.Saque, l.TipoOperacao));
        _cenario.UnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task SaqueRetornaSaldoInsuficienteQuandoSaldoMenorQueValor()
    {
        var conta = _cenario.NovaConta(1);
        _cenario.DarSaldo(conta, 99.99m);

        var resultado = await _service.SacarAsync(Operacao(1, 100m), CenarioDeServico.IdDono, eAdmin: false);

        Assert.True(resultado.HasError<SaldoInsuficienteError>());
        _cenario.VerificarQueNadaFoiGravado();
    }

    // ---- Regras comuns ----

    [Theory]
    [InlineData("deposito")]
    [InlineData("saque")]
    public async Task RetornaNaoEncontradoQuandoAContaEhDeOutroUsuario(string operacao)
    {
        var contaDeOutro = _cenario.NovaConta(1, CenarioDeServico.IdOutroUsuario);
        _cenario.DarSaldo(contaDeOutro, 500m);

        var resultado = await ExecutarAsync(operacao, 1, 10m);

        Assert.True(resultado.HasError<NaoEncontradoError>());
        Assert.Equal("Conta não encontrada", resultado.Errors[0].Message);
        _cenario.VerificarQueNadaFoiGravado();
    }

    [Theory]
    [InlineData("deposito")]
    [InlineData("saque")]
    public async Task RetornaConflitoQuandoAContaEstaEncerrada(string operacao)
    {
        var conta = _cenario.NovaConta(1, encerrada: true);
        _cenario.DarSaldo(conta, 500m);

        var resultado = await ExecutarAsync(operacao, 1, 10m);

        Assert.True(resultado.HasError<ConflitoError>());
        Assert.Equal("A conta foi encerrada", resultado.Errors[0].Message);
        _cenario.VerificarQueNadaFoiGravado();
    }

    [Theory]
    [InlineData("deposito")]
    [InlineData("saque")]
    public async Task AdminNaoMovimentaAContaCaixa(string operacao)
    {
        var resultado = await ExecutarAsync(operacao, CenarioDeServico.NumeroContaCaixa, 10m, "id-do-admin", eAdmin: true);

        Assert.True(resultado.HasError<ConflitoError>());
        Assert.Equal("A conta Caixa é do banco e não pode ser movimentada diretamente", resultado.Errors[0].Message);
        _cenario.VerificarQueNadaFoiGravado();
    }
}
