using Ledger.Domain.Constantes;
using Ledger.Domain.Exceptions;
using Ledger.Domain.ValueObjects;

namespace Ledger.Domain.Entities;

public class Conta
{
    protected Conta() { } //Construtor EF Core por conta dos values objects
    
    public Conta(string nome, Cpf cpf, string idUsuario)
    {
        Nome = nome;
        Cpf = cpf;
        IdUsuario = idUsuario;
        Saldo = new Valor(0);
    }
    
    public int Numero { get;  private set; }
    public string Nome { get; private set; }
    public Cpf Cpf { get; private set; }
    public string IdUsuario { get; private set; }
    public DateTime? DataEncerramento { get; private set; }
    public Valor Saldo { get; private set; }

    private bool EhCaixa => IdUsuario == ContaCaixa.IdUsuario;

    public void Debitar(Valor valor)
    {
        if (valor.Value <= 0) throw new ValorNegativoException();
        if (DataEncerramento.HasValue) throw new ContaEncerradaException();
        if (!EhCaixa && Saldo.Value < valor.Value) throw new SaldoInsuficienteException();
        Saldo = new Valor(Saldo.Value - valor.Value);
    }

    public void Creditar(Valor valor)
    {
        if (valor.Value <= 0) throw new ValorNegativoException();
        if (DataEncerramento.HasValue) throw new ContaEncerradaException();
        Saldo = new Valor(Saldo.Value + valor.Value);
    }

    public void Encerrar()
    {
        if (EhCaixa) throw new OperacaoNaoPermitidaException("Não é possível encerrar a conta caixa");
        if (DataEncerramento.HasValue) throw new ContaEncerradaException();
        if (Saldo.Value != 0) throw new SaldoEmContaExistenteException();
        DataEncerramento = DateTime.UtcNow;
    }

    public void Renomear(string novoNome)
    {
        if (EhCaixa) throw new OperacaoNaoPermitidaException("Não é possível renomear a conta caixa");
        if (string.IsNullOrWhiteSpace(novoNome)) throw new NomeInvalidoException("O nome não pode ser nulo ou vazio");
        if (novoNome == Nome) throw new NomeInvalidoException("O nome precisa ser diferente do nome atual");
        if (DataEncerramento.HasValue) throw new ContaEncerradaException();
        Nome = novoNome;
    }

}