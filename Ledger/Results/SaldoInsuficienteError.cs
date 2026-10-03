using FluentResults;

namespace Ledger.Results;

public class SaldoInsuficienteError : Error
{
    public SaldoInsuficienteError(string mensagem) : base(mensagem)
    {
        
    }
}