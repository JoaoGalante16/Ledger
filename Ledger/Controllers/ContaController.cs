using System.Security.Claims;
using Ledger.Data.ContaDtos;
using Ledger.Services;
using Microsoft.AspNetCore.Authorization;
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
    
    [Authorize]
    [HttpPost]
    public IActionResult CriaConta([FromBody] CreateContaDto dto)
    {
        var idUsuario = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (idUsuario is null) return BadRequest();
        var conta = _contaService.Criar(dto, idUsuario);
        return Ok(conta);    
    }
    
    [Authorize]
    [HttpGet]
    public IActionResult ListarContas()
    {
        bool eAdmin = false;
        var idUsuario = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if(User.IsInRole("Admin")) eAdmin = true;
        var contas = _contaService.Listar(idUsuario, eAdmin);
        return Ok(contas);
    }

    [Authorize]
    [HttpPut("{numero}")]
    public IActionResult AtualizaConta([FromBody] UpdateContaDto dto,int numero)
    {
        var sucesso = _contaService.Atualizar(dto, numero);
        if (sucesso is false) return NotFound();
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{numero}")]
    public IActionResult ExcluirConta(int numero)
    {
        var sucesso = _contaService.Remover(numero);
        if (sucesso is false) return NotFound();
        return NoContent();
    }
    
    [Authorize]
    [HttpGet("{numero}")]
    public IActionResult BuscarContaPorNumero(int numero)
    {
        var conta = _contaService.Buscar(numero);
        if (conta is null) return NotFound();
        return Ok(conta);
    }
}