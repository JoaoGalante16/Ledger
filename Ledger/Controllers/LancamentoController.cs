using Ledger.Data.LancamentoDtos;
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
        var lancamento = _lancamentoService.Criar(dto);
        if (lancamento is null) return NotFound();
        return Ok(lancamento);
    }

    [Authorize]
    [HttpGet]
    public IActionResult Listarlancamentos()
    {
        var lancamento = _lancamentoService.Listar();
        return Ok(lancamento);
    }

    [Authorize]
    [HttpGet("{id}")]
    public IActionResult BuscarLancamentoPorId(int id)
    {
        var lancamento = _lancamentoService.Buscar(id);
        if (lancamento is null) return NotFound();
        return Ok(lancamento);
    }
    
    [Authorize]
    [HttpGet("transacao/{id}")]
    public IActionResult BuscarParDeLancamentoPorIdTransacao(int id)
    {
        var lancamentos = _lancamentoService.BuscarParDeLancamentos(id);
        if (lancamentos is null) return NotFound();
        return Ok(lancamentos);
    }

    [Authorize]
    [HttpPost("correcao")]
    public IActionResult CriarLancamentoDeCorrecao([FromBody] CreateLancamentoCorrecaoDto dto)
    {
        var lancamentoCorrecao = _lancamentoService.CriarLancamentoCorrecao(dto);
        if (lancamentoCorrecao is null) return NotFound();
        return Ok(lancamentoCorrecao);
    }
}