namespace Ledger.Domain.Exceptions;

public class OperacaoNaoPermitidaException : DomainException
{
    public OperacaoNaoPermitidaException(string message) : base(message)
    {
    }
}