
namespace Ledger.Domain.Exceptions;

public class SaldoInsuficienteException : DomainException
{
    public SaldoInsuficienteException() : base("Saldo insuficiente")
    {
    }
}