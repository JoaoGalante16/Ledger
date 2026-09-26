using System.Security.Claims;
using Ledger.Data.LancamentoDtos;
using Ledger.Models;
using Ledger.Results;
using Ledger.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ledger.Controllers;

[ApiController]
[Route("[controller]")]
public class LancamentoController : ControllerBase
{
    private readonly ILancamentoService _lancamentoService;
    
    public LancamentoController(ILancamentoService lancamentoService)
    {
        _lancamentoService = lancamentoService;
    }
    
    [Authorize]
    [HttpPost]
    public IActionResult CriaLancamento([FromBody] CreateLancamentoDto dto)
    {
        var resultado = _lancamentoService.Criar(dto, User.ObterId(), User.EhAdmin());
        if (resultado.HasError<NaoEncontradoError>()) return NotFound(resultado.Errors.First().Message);
        if (resultado.IsFailed) return Problem();
        return Ok(resultado.Value);
    }

    [Authorize]
    [HttpGet]
    public IActionResult Listarlancamentos()
    {
        var lancamento = _lancamentoService.Listar(User.ObterId(), User.EhAdmin());
        return Ok(lancamento);
    }

    [Authorize]
    [HttpGet("{id}")]
    public IActionResult BuscarLancamentoPorId(int id)
    {
        var resultado = _lancamentoService.Buscar(id, User.ObterId(), User.EhAdmin());
        if (resultado.HasError<NaoEncontradoError>()) return NotFound(resultado.Errors.First().Message);
        if (resultado.IsFailed) return Problem();
        return Ok(resultado.Value);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpGet("transacao/{id}")]
    public IActionResult BuscarParDeLancamentoPorIdTransacao(int id)
    {
        var resultado = _lancamentoService.BuscarParDeLancamentos(id);
        if (resultado.HasError<NaoEncontradoError>()) return NotFound(resultado.Errors.First().Message);
        if (resultado.IsFailed) return Problem();
        return Ok(resultado.Value);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("correcao")]
    public IActionResult CriarLancamentoDeCorrecao([FromBody] CreateLancamentoCorrecaoDto dto)
    {
        var resultado = _lancamentoService.CriarLancamentoCorrecao(dto);
        if (resultado.HasError<NaoEncontradoError>()) return NotFound(resultado.Errors.First().Message);
        if (resultado.IsFailed) return Problem();
        return Ok(resultado.Value);
    }
}