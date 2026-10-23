using System.Text.Json;
using Ledger.Api.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ledger.Tests.Unitarios.Api;

public class GlobalExceptionHandlerTestes
{
    // ---- Exceção não tratada ----

    [Fact]
    public async Task DevolveProblemDetails500SemAMensagemDaExcecao()
    {
        //monta requisicao e excecao falsa
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);
        var contexto = new DefaultHttpContext();
        contexto.Response.Body = new MemoryStream();
        var excecao = new InvalidOperationException("senha do banco: 123456");
        
        var tratou = await handler.TryHandleAsync(contexto, excecao, CancellationToken.None);
        
        Assert.True(tratou);
        Assert.Equal(StatusCodes.Status500InternalServerError, contexto.Response.StatusCode);

        contexto.Response.Body.Position = 0;
        var corpo = await new StreamReader(contexto.Response.Body).ReadToEndAsync(TestContext.Current.CancellationToken);
        var problema = JsonSerializer.Deserialize<ProblemDetails>(corpo, JsonSerializerOptions.Web);

        Assert.NotNull(problema);
        Assert.Equal(500, problema.Status);
        Assert.Equal("Ocorreu um erro inesperado", problema.Title);
        Assert.DoesNotContain("123456", corpo);
        Assert.DoesNotContain("senha", corpo);
    }
}
