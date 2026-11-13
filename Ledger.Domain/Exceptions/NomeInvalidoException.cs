namespace Ledger.Domain.Exceptions;

public class NomeInvalidoException : DomainException
{
    public NomeInvalidoException(string message) : base (message)
    {
    }
}