using Ledger.Data.LancamentoDtos;
using Ledger.Services;
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
    
    [HttpPost]
    public IActionResult CriaLancamento([FromBody] CreateLancamentoDto dto)
    {
        var lancamento = _lancamentoService.Criar(dto);
        return Ok(lancamento);
    }

    // Metodo criado com minimal API
    // [HttpGet]
    // public IActionResult Listarlancamentos()
    // {
    //     var lancamento = _lancamentoService.Listar();
    //     return Ok(lancamento);
    // }

    [HttpGet("{id}")]
    public IActionResult BuscarLancamentoPorId(int id)
    {
        var lancamento = _lancamentoService.Buscar(id);
        if (lancamento is null) return NotFound();
        return Ok(lancamento);
    }
    
    [HttpGet("transacao/{id}")]
    public IActionResult BuscarParDeLancamentoPorIdTransacao(int id)
    {
        var lancamentos = _lancamentoService.BuscarParDeLancamentos(id);
        if (lancamentos is null) return NotFound();
        return Ok(lancamentos);
    }
}