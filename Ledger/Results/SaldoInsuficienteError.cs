using FluentResults;

namespace Ledger.Results;

public class SaldoInsuficienteError : Error
{
    public SaldoInsuficienteError(string mensage) : base(mensage)
    {
        
    }
}