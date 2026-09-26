using System.Security.Claims;
using Ledger.Data.ContaDtos;
using Ledger.Models;
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
        var conta = _contaService.Criar(dto, User.ObterId());
        if(conta is null) return NotFound();
        return Ok(conta);    
    }
    
    [Authorize]
    [HttpGet]
    public IActionResult ListarContas()
    {
        var contas = _contaService.Listar(User.ObterId(), User.EhAdmin());
        if(contas is null) return NotFound();
        return Ok(contas);
    }

    [Authorize]
    [HttpPut("{numero}")]
    public IActionResult AtualizaConta([FromBody] UpdateContaDto dto,int numero)
    {
        var sucesso = _contaService.Atualizar(dto, numero, User.ObterId(), User.EhAdmin());
        if (sucesso is false) return NotFound();
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{numero}")]
    public IActionResult ExcluirConta(int numero)
    {
        var sucesso = _contaService.Remover(numero,  User.ObterId(), User.EhAdmin());
        if (sucesso is false) return NotFound();
        return NoContent();
    }
    
    [Authorize]
    [HttpGet("{numero}")]
    public IActionResult BuscarContaPorNumero(int numero)
    {
        var conta = _contaService.Buscar(numero, User.ObterId(), User.EhAdmin());
        if (conta is null) return NotFound();
        return Ok(conta);
    }
}