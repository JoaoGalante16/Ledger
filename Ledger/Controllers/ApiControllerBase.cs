using FluentResults;
using Ledger.Results;
using Microsoft.AspNetCore.Mvc;

namespace Ledger.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult ProblemaDe(ResultBase resultado)
    {
        var detalhe = resultado.Errors.First().Message;

        if (resultado.HasError<NaoEncontradoError>())
            return Problem(detail: detalhe, statusCode: StatusCodes.Status404NotFound);

        return Problem();
    }
}