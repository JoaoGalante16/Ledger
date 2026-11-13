using System.Text.Json;
using Ledger.Api.Handlers;
using Ledger.Domain.Exceptions;
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

    // ---- Regra de negócio ----

    public static TheoryData<DomainException, int> ExcecoesDeDominio => new()
    {
        { new SaldoInsuficienteException(), StatusCodes.Status400BadRequest },
        { new ValorNegativoException(), StatusCodes.Status400BadRequest },
        { new NomeInvalidoException("O nome não pode ser nulo ou vazio"), StatusCodes.Status400BadRequest },
        { new ContaEncerradaException(), StatusCodes.Status409Conflict },
        { new SaldoEmContaExistenteException(), StatusCodes.Status409Conflict },
        { new OperacaoNaoPermitidaException("Não é possível encerrar a conta caixa"), StatusCodes.Status409Conflict }
    };

    [Theory]
    [MemberData(nameof(ExcecoesDeDominio))]
    public async Task RegraDeNegocioViraProblemDetailsComAMensagemDaExcecao(DomainException excecao, int statusEsperado)
    {
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);
        var contexto = new DefaultHttpContext();
        contexto.Response.Body = new MemoryStream();

        var tratou = await handler.TryHandleAsync(contexto, excecao, CancellationToken.None);

        Assert.True(tratou);
        Assert.Equal(statusEsperado, contexto.Response.StatusCode);

        contexto.Response.Body.Position = 0;
        var corpo = await new StreamReader(contexto.Response.Body).ReadToEndAsync(TestContext.Current.CancellationToken);
        var problema = JsonSerializer.Deserialize<ProblemDetails>(corpo, JsonSerializerOptions.Web);

        Assert.NotNull(problema);
        Assert.Equal(statusEsperado, problema.Status);
        Assert.Equal(excecao.Message, problema.Detail);
    }
}
