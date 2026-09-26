using FluentResults;

namespace Ledger.Results;

public class NaoEncontradoError : Error
{
    public NaoEncontradoError(string mensagem) : base(mensagem)
    {
        
    }
}