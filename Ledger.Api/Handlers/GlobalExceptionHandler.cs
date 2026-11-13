using Ledger.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Ledger.Api.Handlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    
    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }
    
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is DomainException domainException)
        {
            var status = domainException switch
            {
                SaldoInsuficienteException => StatusCodes.Status400BadRequest,
                ValorNegativoException => StatusCodes.Status400BadRequest,
                NomeInvalidoException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status409Conflict
            };
            
            _logger.LogWarning("Regra de negócio violada: {Mensagem}", domainException.Message);

            var problemaDeNegocio = new ProblemDetails
            {
                Status = status,
                Title = "A operação não pode ser realizada",
                Detail = domainException.Message
            };

            httpContext.Response.StatusCode = status;
            await httpContext.Response.WriteAsJsonAsync(problemaDeNegocio, cancellationToken);
            return true;
        }
        
        _logger.LogError(exception, "Erro inesperado na requisição");

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Ocorreu um erro inesperado"
        };

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}