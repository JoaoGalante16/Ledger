using Ledger.Data.Dtos.ContaDtos;
using Ledger.Models;
using Ledger.Results;
using Ledger.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Ledger.Controllers;

[ApiController]
[Route("[controller]")]
public class ContaController : ApiControllerBase
{
    private readonly IContaService _contaService;

    public ContaController(IContaService contaService)
    {
        _contaService = contaService;
    }
    
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CriaConta([FromBody] CreateContaDto dto)
    {
        var conta = await _contaService.Criar(dto, User.ObterId());
        return Ok(conta);  
    }
    
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> ListarContas()
    {
        var contas = await _contaService.Listar(User.ObterId(), User.EhAdmin());
        return Ok(contas);
    }

    [Authorize]
    [HttpPut("{numero}")]
    public async Task<IActionResult> AtualizaConta([FromBody] UpdateContaDto dto,int numero)
    {
        var resultado = await _contaService.Atualizar(dto, numero, User.ObterId(), User.EhAdmin());
        if (resultado.IsFailed) return ProblemaDe(resultado);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{numero}")]
    public async Task<IActionResult> ExcluirConta(int numero)
    {
        var resultado = await _contaService.Remover(numero,  User.ObterId(), User.EhAdmin());
        if (resultado.IsFailed) return ProblemaDe(resultado);
        return NoContent();
    }
    
    [Authorize]
    [HttpGet("{numero}")]
    public async Task<IActionResult> BuscarContaPorNumero(int numero)
    {
        var resultado = await _contaService.Buscar(numero, User.ObterId(), User.EhAdmin());
        if (resultado.IsFailed) return ProblemaDe(resultado);
        return Ok(resultado.Value);
    }

    [Authorize]
    [HttpPost("{numero}/encerrar")]
    public async Task<IActionResult> EncerrarConta(int numero)
    {
        var resultado = await _contaService.Encerrar(numero, User.ObterId(), User.EhAdmin());
        if (resultado.IsFailed) return ProblemaDe(resultado);
        return NoContent();
    }
}