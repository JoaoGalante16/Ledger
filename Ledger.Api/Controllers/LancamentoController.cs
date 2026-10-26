using Ledger.Application.Services;
using Ledger.Api.Extensions;
using Ledger.Application.Dtos.Lancamentos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ledger.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class LancamentoController : ApiControllerBase
{
    private readonly ILancamentoService _lancamentoService;
    
    public LancamentoController(ILancamentoService lancamentoService)
    {
        _lancamentoService = lancamentoService;
    }
    
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CriaLancamento([FromBody] CreateLancamentoDto dto)
    {
        var resultado = await _lancamentoService.CriarAsync(dto, User.ObterId(), User.EhAdmin());
        if (resultado.IsFailed) return ProblemaDe(resultado);
        return Ok(resultado.Value);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> ListarLancamentos()
    {
        var lancamento = await _lancamentoService.ListarAsync(User.ObterId(), User.EhAdmin());
        return Ok(lancamento);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> BuscarLancamentoPorId(int id)
    {
        var resultado = await _lancamentoService.BuscarAsync(id, User.ObterId(), User.EhAdmin());
        if (resultado.IsFailed) return ProblemaDe(resultado);
        return Ok(resultado.Value);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpGet("transacao/{id}")]
    public async Task<IActionResult> BuscarParDeLancamentoPorIdTransacao(int id)
    {
        var resultado = await _lancamentoService.BuscarParDeLancamentosAsync(id);
        if (resultado.IsFailed) return ProblemaDe(resultado);
        return Ok(resultado.Value);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("correcao")]
    public async Task<IActionResult> CriarLancamentoDeCorrecao([FromBody] CreateLancamentoCorrecaoDto dto)
    {
        var resultado = await _lancamentoService.CriarLancamentoCorrecaoAsync(dto);
        if (resultado.IsFailed) return ProblemaDe(resultado);
        return Ok(resultado.Value);
    }
    
    [Authorize]
    [HttpPost("deposito")]
    public async Task<IActionResult> Depositar([FromBody] CreateLancamentoDepositoOuSaque dto)
    {
        var resultado = await _lancamentoService.DepositarAsync(dto, User.ObterId(), User.EhAdmin());
        if (resultado.IsFailed) return ProblemaDe(resultado);
        return Ok(resultado.Value);
    }
    
    [Authorize]
    [HttpPost("saque")]
    public async Task<IActionResult> Sacar([FromBody] CreateLancamentoDepositoOuSaque dto)
    {
        var resultado = await _lancamentoService.SacarAsync(dto, User.ObterId(), User.EhAdmin());
        if (resultado.IsFailed) return ProblemaDe(resultado);
        return Ok(resultado.Value);
    }
}