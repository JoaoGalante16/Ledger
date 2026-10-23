using FluentResults;

namespace Ledger.Application.Errors;

public class SaldoInsuficienteError : Error
{
    public SaldoInsuficienteError(string mensagem) : base(mensagem)
    {
        
    }
}