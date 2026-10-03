using FluentResults;
using Microsoft.AspNetCore.Identity;

namespace Ledger.Results;

public class ConflitoError : Error
{
    public ConflitoError(string message) : base(message)
    {
    }
}