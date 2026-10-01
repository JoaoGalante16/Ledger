using System.Runtime.InteropServices.JavaScript;
using AutoMapper;
using FluentResults;
using Ledger.Data;
using Ledger.Data.Dtos.ContaDtos;
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
        return _mapper.Map<List<ReadContaDto>>(contas);
    }

    public async Task<Result> Atualizar(UpdateContaDto dto, int id, string idUsuario, bool eAdmin)
    {
        Conta? conta;
        if (eAdmin)
        {
            conta = await _unitOfWork.ContaRepository.BuscarPorPk(c => c.Numero == id);
        }
        else
        {
            conta = await _unitOfWork.ContaRepository.BuscarPorPk(c => c.Numero.Equals(id) && c.IdUsuario == idUsuario);
        }
        if (conta is null) return Result.Fail(new NaoEncontradoError("Conta não encontrada"));
        _mapper.Map(dto, conta);
        await _unitOfWork.ContaRepository.Atualizar(conta);
        await _unitOfWork.Commit();
        return Result.Ok();

    }

    public async Task<Result> Remover(int id, string idUsuario, bool eAdmin)
    {
        Conta? conta;
        if (eAdmin)
        {
            conta = await _unitOfWork.ContaRepository.BuscarPorPk(c=>c.Numero.Equals(id));
        }
        else
        {
            conta = await _unitOfWork.ContaRepository.BuscarPorPk(c => c.Numero.Equals(id) && c.IdUsuario == idUsuario);
        }
        if (conta is null) return Result.Fail(new NaoEncontradoError("Conta não encontrada"));
        await _unitOfWork.ContaRepository.Deletar(conta);
        await _unitOfWork.Commit();
        return Result.Ok();
    }

    public async Task<Result<ReadContaDto>> Buscar(int id, string idUsuario, bool eAdmin)
    {
        Conta? conta;
        if (eAdmin)
        {
            conta = await _unitOfWork.ContaRepository.BuscarPorPk(c => c.Numero.Equals(id));
        }
        else
        {
            conta = await _unitOfWork.ContaRepository.BuscarPorPk(c => c.Numero.Equals(id) && c.IdUsuario == idUsuario);
        }
        if (conta is null) return Result.Fail(new NaoEncontradoError("Conta não encontrada"));;
        var readDto = _mapper.Map<ReadContaDto>(conta);
        return Result.Ok(readDto);
    }
}