using Ledger.Domain.Constantes;
using Ledger.Domain.Entities;
using Ledger.Domain.Exceptions;
using Ledger.Domain.ValueObjects;

namespace Ledger.Tests.Unitarios.Entities;

public class ContaTestes
{
    private static Conta NovaConta(decimal saldo = 0m)
    {
        var conta = new Conta("Conta corrente", new Cpf("52998224725"), "usuario-dono");
        if (saldo > 0) conta.Creditar(new Valor(saldo));
        return conta;
    }

    private static Conta NovaCaixa() => new(ContaCaixa.Nome, new Cpf(ContaCaixa.Cpf), ContaCaixa.IdUsuario);

    // ---- Criação ----

    [Fact]
    public void ContaNovaComecaComSaldoZeroEAberta()
    {
        var conta = NovaConta();

        Assert.Equal(0m, conta.Saldo.Value);
        Assert.Null(conta.DataEncerramento);
    }

    // ---- Débito e crédito ----

    [Fact]
    public void CreditarEDebitarAtualizamOSaldo()
    {
        var conta = NovaConta(300m);

        conta.Debitar(new Valor(120.50m));

        Assert.Equal(179.50m, conta.Saldo.Value);
    }

    [Fact]
    public void DebitarRecusaQuandoFaltaSaldo()
    {
        var conta = NovaConta(99.99m);

        Assert.Throws<SaldoInsuficienteException>(() => conta.Debitar(new Valor(100m)));
        Assert.Equal(99.99m, conta.Saldo.Value);
    }

    [Fact]
    public void CaixaPodeFicarComSaldoNegativo()
    {
        var caixa = NovaCaixa();

        caixa.Debitar(new Valor(500m));

        Assert.Equal(-500m, caixa.Saldo.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void RecusaValorZeroOuNegativo(int valor)
    {
        var conta = NovaConta(100m);

        Assert.Throws<ValorNegativoException>(() => conta.Debitar(new Valor(valor)));
        Assert.Throws<ValorNegativoException>(() => conta.Creditar(new Valor(valor)));
        Assert.Equal(100m, conta.Saldo.Value);
    }

    [Fact]
    public void ContaEncerradaNaoMovimenta()
    {
        var conta = NovaConta();
        conta.Encerrar();

        Assert.Throws<ContaEncerradaException>(() => conta.Creditar(new Valor(10m)));
        Assert.Throws<ContaEncerradaException>(() => conta.Debitar(new Valor(10m)));
    }

    // ---- Encerramento ----

    [Fact]
    public void EncerrarPreencheADataQuandoSaldoEhZero()
    {
        var conta = NovaConta();

        conta.Encerrar();

        Assert.NotNull(conta.DataEncerramento);
    }

    [Fact]
    public void EncerrarRecusaContaComSaldo()
    {
        var conta = NovaConta(0.01m);

        Assert.Throws<SaldoEmContaExistenteException>(() => conta.Encerrar());
        Assert.Null(conta.DataEncerramento);
    }

    [Fact]
    public void EncerrarRecusaContaJaEncerrada()
    {
        var conta = NovaConta();
        conta.Encerrar();

        Assert.Throws<ContaEncerradaException>(() => conta.Encerrar());
    }

    [Fact]
    public void CaixaNaoPodeSerEncerradaNemRenomeada()
    {
        var caixa = NovaCaixa();

        Assert.Throws<OperacaoNaoPermitidaException>(() => caixa.Encerrar());
        Assert.Throws<OperacaoNaoPermitidaException>(() => caixa.Renomear("Outro nome"));
    }

    // ---- Renomear ----

    [Fact]
    public void RenomearTrocaONome()
    {
        var conta = NovaConta();

        conta.Renomear("Poupança");

        Assert.Equal("Poupança", conta.Nome);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Conta corrente")]
    public void RenomearRecusaNomeVazioOuIgualAoAtual(string novoNome)
    {
        var conta = NovaConta();

        Assert.Throws<NomeInvalidoException>(() => conta.Renomear(novoNome));
        Assert.Equal("Conta corrente", conta.Nome);
    }

    [Fact]
    public void RenomearRecusaContaEncerrada()
    {
        var conta = NovaConta();
        conta.Encerrar();

        Assert.Throws<ContaEncerradaException>(() => conta.Renomear("Poupança"));
    }
}
