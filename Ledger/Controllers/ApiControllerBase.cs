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
        if (resultado.HasError<SaldoInsuficienteError>())
            return Problem(detail: detalhe, statusCode: StatusCodes.Status400BadRequest);
        if (resultado.HasError<ConflitoError>())
            return Problem(detail: detalhe, statusCode: StatusCodes.Status409Conflict);

        return Problem();
    }
}