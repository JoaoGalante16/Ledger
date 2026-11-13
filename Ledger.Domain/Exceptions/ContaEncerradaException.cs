namespace Ledger.Domain.Exceptions;

public class ContaEncerradaException : DomainException
{
    public ContaEncerradaException() : base("Conta encerrada")
    {
    }
}


