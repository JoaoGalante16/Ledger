using System.Runtime.InteropServices.JavaScript;
using AutoMapper;
using FluentResults;
using Ledger.Data;
using Ledger.Data.Dtos.ContaDtos;
using Ledger.Data.Dtos.LancamentoDtos;
using Ledger.Data.UnitOfWork;
using Ledger.Models;
using Ledger.Results;
using Microsoft.EntityFrameworkCore;

namespace Ledger.Services;

public class ContaService : IContaService
{
    private readonly IUnitOfWork  _unitOfWork;
    private readonly IMapper _mapper;

    public ContaService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ReadContaDto> Criar(CreateContaDto dto, string idUsuario)
    {
        var conta = _mapper.Map<Conta>(dto);
        conta.IdUsuario = idUsuario;
        await _unitOfWork.ContaRepository.Adicionar(conta);
        await _unitOfWork.Commit();
        return _mapper.Map<ReadContaDto>(conta);

    }

    public async Task<List<ReadContaDto>> Listar(string idUsuario, bool eAdmin)
    {
        var query = await _unitOfWork.ContaRepository.BuscarTodos();
        if (!eAdmin)
        {
            query = query.Where(c => c.IdUsuario == idUsuario);
        }
        var contas = await query.ToListAsync();

        var numerosContas = contas.Select(c => c.Numero).ToList();
        var queryLancamentos = await _unitOfWork.LancamentoRepository.BuscarTodos();
        var saldos = queryLancamentos.Where(l => numerosContas.Contains(l.NumeroConta))
            .GroupBy(l => l.NumeroConta)
            .Select(g => new { NumeroConta = g.Key, Saldo = g.Sum(l => l.Valor) })
            .ToDictionary(x => x.NumeroConta, x => x.Saldo);
        
        var readDto = _mapper.Map<List<ReadContaDto>>(contas);
        foreach (var conta in readDto)
        {
            conta.Saldo = saldos.GetValueOrDefault(conta.Numero);
        }
        return readDto;
    }

    public async Task<Result> Atualizar(UpdateContaDto dto, int id, string idUsuario, bool eAdmin)
    {
        var conta = await BuscarContaAcessivel(id, idUsuario, eAdmin);
        if (conta is null) return Result.Fail(new NaoEncontradoError("Conta não encontrada"));
        _mapper.Map(dto, conta);
        await _unitOfWork.ContaRepository.Atualizar(conta);
        await _unitOfWork.Commit();
        return Result.Ok();

    }

    public async Task<Result> Remover(int id, string idUsuario, bool eAdmin)
    {
        var conta = await BuscarContaAcessivel(id, idUsuario, eAdmin);
        if (conta is null) return Result.Fail(new NaoEncontradoError("Conta não encontrada"));

        var querry = await _unitOfWork.LancamentoRepository.BuscarTodos();
        var temLancamento = await querry.AnyAsync(l => l.NumeroConta == id);
        if (temLancamento)
            return Result.Fail(new ConflitoError("A conta tem lançamentos registrados, use a opção de desativar"));
        
        await _unitOfWork.ContaRepository.Deletar(conta);
        await _unitOfWork.Commit();
        return Result.Ok();
    }

    public async Task<Result<ReadContaDto>> Buscar(int id, string idUsuario, bool eAdmin)
    {
        var conta = await BuscarContaAcessivel(id, idUsuario, eAdmin);
        if (conta is null) return Result.Fail(new NaoEncontradoError("Conta não encontrada"));
        var query = await _unitOfWork.LancamentoRepository.BuscarTodos();
        var saldo = await query.Where(l => l.NumeroConta == id).SumAsync(l => l.Valor);
        var readDto = _mapper.Map<ReadContaDto>(conta);
        readDto.Saldo = saldo;
        return Result.Ok(readDto);
    }
    
    private Task<Conta?> BuscarContaAcessivel(int id, string idUsuario, bool eAdmin)
    {
        return _unitOfWork.ContaRepository.BuscarPorPk(c =>
            c.Numero == id && (eAdmin || c.IdUsuario == idUsuario));
    }
}