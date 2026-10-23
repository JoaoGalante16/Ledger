using FluentResults;
using Ledger.Api.Controllers;
using Ledger.Application.Errors;
using Microsoft.AspNetCore.Mvc;

namespace Ledger.Tests.Unitarios.Api;

public class ApiControllerBaseTestes
{
    private class ControllerDeTeste : ApiControllerBase
    {
        public IActionResult Chamar(ResultBase resultado) => ProblemaDe(resultado);
    }

    private static ProblemDetails Converter(Error erro, out int? status)
    {
        var resposta = Assert.IsType<ObjectResult>(new ControllerDeTeste().Chamar(Result.Fail(erro)));
        status = resposta.StatusCode;
        return Assert.IsType<ProblemDetails>(resposta.Value);
    }

    // ---- Erros conhecidos ----

    [Fact]
    public void NaoEncontradoViraStatus404()
    {
        var problema = Converter(new NaoEncontradoError("Conta não encontrada"), out var status);

        Assert.Equal(404, status);
        Assert.Equal("Conta não encontrada", problema.Detail);
    }

    [Fact]
    public void SaldoInsuficienteViraStatus400()
    {
        var problema = Converter(new SaldoInsuficienteError("Saldo insuficiente"), out var status);

        Assert.Equal(400, status);
        Assert.Equal("Saldo insuficiente", problema.Detail);
    }

    [Fact]
    public void ConflitoViraStatus409()
    {
        var problema = Converter(new ConflitoError("A conta já está encerrada"), out var status);

        Assert.Equal(409, status);
        Assert.Equal("A conta já está encerrada", problema.Detail);
    }

    // ---- Erro desconhecido ----

    [Fact]
    public void ErroDesconhecidoViraStatus500SemExporAMensagem()
    {
        var problema = Converter(new Error("detalhe interno"), out var status);

        Assert.Equal(500, status);
        Assert.Null(problema.Detail);
    }
}
