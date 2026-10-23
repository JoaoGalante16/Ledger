using FluentResults;

namespace Ledger.Application.Errors;

public class ConflitoError : Error
{
    public ConflitoError(string mensagem) : base(mensagem)
    {
    }
}