using System.Security.Claims;
using Ledger.Data.Dtos.LancamentoDtos;
using Ledger.Models;
using Ledger.Results;
using Ledger.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ledger.Controllers;

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
        var resultado = await _lancamentoService.Criar(dto, User.ObterId(), User.EhAdmin());
        if (resultado.IsFailed) return ProblemaDe(resultado);
        return Ok(resultado.Value);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Listarlancamentos()
    {
        var lancamento = await _lancamentoService.Listar(User.ObterId(), User.EhAdmin());
        return Ok(lancamento);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> BuscarLancamentoPorId(int id)
    {
        var resultado = await _lancamentoService.Buscar(id, User.ObterId(), User.EhAdmin());
        if (resultado.IsFailed) return ProblemaDe(resultado);
        return Ok(resultado.Value);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpGet("transacao/{id}")]
    public async Task<IActionResult> BuscarParDeLancamentoPorIdTransacao(int id)
    {
        var resultado = await _lancamentoService.BuscarParDeLancamentos(id);
        if (resultado.IsFailed) return ProblemaDe(resultado);
        return Ok(resultado.Value);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("correcao")]
    public async Task<IActionResult> CriarLancamentoDeCorrecao([FromBody] CreateLancamentoCorrecaoDto dto)
    {
        var resultado = await _lancamentoService.CriarLancamentoCorrecao(dto);
        if (resultado.IsFailed) return ProblemaDe(resultado);
        return Ok(resultado.Value);
    }
}