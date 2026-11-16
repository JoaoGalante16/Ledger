using Ledger.Api.Extensions;
using Ledger.Application.Dtos.Contas;
using Ledger.Application.UseCases.Contas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ledger.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ContaController : ApiControllerBase
{
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CriaConta([FromBody] CreateContaDto dto, [FromServices] CriarConta criarConta)
    {
        var conta = await criarConta.ExecutarAsync(dto, User.ObterId());
        return Ok(conta);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> ListarContas([FromServices] ListarContas listarContas)
    {
        var contas = await listarContas.ExecutarAsync(User.ObterId(), User.EhAdmin());
        return Ok(contas);
    }

    [Authorize]
    [HttpPut("{numero}")]
    public async Task<IActionResult> AtualizaConta([FromBody] UpdateContaDto dto, int numero, [FromServices] AtualizarConta atualizarConta)
    {
        var resultado = await atualizarConta.ExecutarAsync(dto, numero, User.ObterId(), User.EhAdmin());
        if (resultado.IsFailed) return ProblemaDe(resultado);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{numero}")]
    public async Task<IActionResult> ExcluirConta(int numero, [FromServices] RemoverConta removerConta)
    {
        var resultado = await removerConta.ExecutarAsync(numero, User.ObterId(), User.EhAdmin());
        if (resultado.IsFailed) return ProblemaDe(resultado);
        return NoContent();
    }

    [Authorize]
    [HttpGet("{numero}")]
    public async Task<IActionResult> BuscarContaPorNumero(int numero, [FromServices] BuscarConta buscarConta)
    {
        var resultado = await buscarConta.ExecutarAsync(numero, User.ObterId(), User.EhAdmin());
        if (resultado.IsFailed) return ProblemaDe(resultado);
        return Ok(resultado.Value);
    }

    [Authorize]
    [HttpPost("{numero}/encerrar")]
    public async Task<IActionResult> EncerrarConta(int numero, [FromServices] EncerrarConta encerrarConta)
    {
        var resultado = await encerrarConta.ExecutarAsync(numero, User.ObterId(), User.EhAdmin());
        if (resultado.IsFailed) return ProblemaDe(resultado);
        return NoContent();
    }
}
