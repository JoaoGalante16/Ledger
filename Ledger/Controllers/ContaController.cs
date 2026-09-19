using Ledger.Data.ContaDtos;
using Ledger.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ledger.Controllers;

[ApiController]
[Route("[controller]")]
public class ContaController : ControllerBase
{
    private readonly IContaService _contaService;

    public ContaController(IContaService contaService)
    {
        _contaService = contaService;
    }
    
    // Metodo criado com minimal API
    // [HttpPost]
    // public IActionResult CriaConta([FromBody] CreateContaDto dto)
    // {
    //     var conta = _contaService.Criar(dto);
    //     return Ok(conta);    
    // }
    
    [HttpGet]
    public IActionResult ListarContas()
    {
        var contas = _contaService.Listar();
        return Ok(contas);
    }

    [HttpPut("{numero}")]
    public IActionResult AtualizaConta([FromBody] UpdateContaDto dto,int numero)
    {
        var sucesso = _contaService.Atualizar(dto, numero);
        if (sucesso is false) return NotFound();
        return NoContent();
    }

    [HttpDelete("{numero}")]
    public IActionResult ExcluirConta(int numero)
    {
        var sucesso = _contaService.Remover(numero);
        if (sucesso is false) return NotFound();
        return NoContent();
    }
    
    [HttpGet("{numero}")]
    public IActionResult BuscarContaPorNumero(int numero)
    {
        var conta = _contaService.Buscar(numero);
        if (conta is null) return NotFound();
        return Ok(conta);
    }
}