namespace Ledger.Domain.Exceptions;

public class SaldoEmContaExistenteException :DomainException
{
    public SaldoEmContaExistenteException() :base("Existe saldo disponível na conta")
    {
    }
}