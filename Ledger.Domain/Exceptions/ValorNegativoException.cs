namespace Ledger.Domain.Exceptions;

public class ValorNegativoException : DomainException
{
    public ValorNegativoException() : base("O valor deve ser maior que zero")
    {
        
    }
}