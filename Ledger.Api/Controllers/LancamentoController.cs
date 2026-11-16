using Ledger.Api.Extensions;
using Ledger.Application.Dtos.Lancamentos;
using Ledger.Application.UseCases.Lancamentos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ledger.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class LancamentoController : ApiControllerBase
{
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CriaLancamento([FromBody] CreateLancamentoDto dto, [FromServices] Transferir transferir)
    {
        var resultado = await transferir.ExecutarAsync(dto, User.ObterId(), User.EhAdmin());
        if (resultado.IsFailed) return ProblemaDe(resultado);
        return Ok(resultado.Value);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> ListarLancamentos([FromServices] ListarLancamentos listarLancamentos)
    {
        var lancamentos = await listarLancamentos.ExecutarAsync(User.ObterId(), User.EhAdmin());
        return Ok(lancamentos);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> BuscarLancamentoPorId(int id, [FromServices] BuscarLancamento buscarLancamento)
    {
        var resultado = await buscarLancamento.ExecutarAsync(id, User.ObterId(), User.EhAdmin());
        if (resultado.IsFailed) return ProblemaDe(resultado);
        return Ok(resultado.Value);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("transacao/{id}")]
    public async Task<IActionResult> BuscarParDeLancamentoPorIdTransacao(int id, [FromServices] BuscarParDaTransacao buscarParDaTransacao)
    {
        var resultado = await buscarParDaTransacao.ExecutarAsync(id);
        if (resultado.IsFailed) return ProblemaDe(resultado);
        return Ok(resultado.Value);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("correcao")]
    public async Task<IActionResult> CriarLancamentoDeCorrecao([FromBody] CreateLancamentoCorrecaoDto dto, [FromServices] CorrigirLancamento corrigirLancamento)
    {
        var resultado = await corrigirLancamento.ExecutarAsync(dto);
        if (resultado.IsFailed) return ProblemaDe(resultado);
        return Ok(resultado.Value);
    }

    [Authorize]
    [HttpPost("deposito")]
    public async Task<IActionResult> Depositar([FromBody] CreateLancamentoDepositoOuSaque dto, [FromServices] Depositar depositar)
    {
        var resultado = await depositar.ExecutarAsync(dto, User.ObterId(), User.EhAdmin());
        if (resultado.IsFailed) return ProblemaDe(resultado);
        return Ok(resultado.Value);
    }

    [Authorize]
    [HttpPost("saque")]
    public async Task<IActionResult> Sacar([FromBody] CreateLancamentoDepositoOuSaque dto, [FromServices] Sacar sacar)
    {
        var resultado = await sacar.ExecutarAsync(dto, User.ObterId(), User.EhAdmin());
        if (resultado.IsFailed) return ProblemaDe(resultado);
        return Ok(resultado.Value);
    }
}
