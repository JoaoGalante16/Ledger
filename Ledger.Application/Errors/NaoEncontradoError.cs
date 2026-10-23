using FluentResults;

namespace Ledger.Application.Errors;

public class NaoEncontradoError : Error
{
    public NaoEncontradoError(string mensagem) : base(mensagem)
    {
        
    }
}