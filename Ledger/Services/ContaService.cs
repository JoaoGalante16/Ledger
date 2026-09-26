using System.Runtime.InteropServices.JavaScript;
using AutoMapper;
using FluentResults;
using Ledger.Data;
using Ledger.Data.ContaDtos;
using Ledger.Models;
using Ledger.Results;

namespace Ledger.Services;

public class ContaService : IContaService
{
    private readonly LedgerContext _context;
    private readonly IMapper _mapper;

    public ContaService(LedgerContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public ReadContaDto Criar(CreateContaDto dto,  string idUsuario)
    {
        var conta = _mapper.Map<Conta>(dto);
        conta.IdUsuario = idUsuario;
        _context.Contas.Add(conta);
        _context.SaveChanges();
        return _mapper.Map<ReadContaDto>(conta);

    }

    public List<ReadContaDto> Listar(string idUsuario, bool eAdmin)
    {
        if (eAdmin)
        {
            return _mapper.Map<List<ReadContaDto>>(_context.Contas.ToList());
        }
        return _mapper.Map<List<ReadContaDto>>(_context.Contas.Where(c => c.IdUsuario == idUsuario).ToList());
    }

    public Result Atualizar(UpdateContaDto dto, int id, string idUsuario, bool eAdmin)
    {
        Conta? conta;
        if (eAdmin)
        {
            conta = _context.Contas.FirstOrDefault(c =>  c.Numero.Equals(id));
        }
        else
        {
            conta = _context.Contas.FirstOrDefault(c => c.Numero.Equals(id) && c.IdUsuario == idUsuario);
        }
        if (conta is null) return Result.Fail(new NaoEncontradoError("Conta não encontrada"));
        _mapper.Map(dto, conta);
        _context.SaveChanges();
        return Result.Ok();

    }

    public Result Remover(int id, string idUsuario, bool eAdmin)
    {
        Conta? conta;
        if (eAdmin)
        {
            conta = _context.Contas.FirstOrDefault(c=>c.Numero.Equals(id));
        }
        else
        {
            conta = _context.Contas.FirstOrDefault(c => c.Numero.Equals(id) && c.IdUsuario == idUsuario);
        }
        if (conta is null) return Result.Fail(new NaoEncontradoError("Conta não encontrada"));
        _context.Contas.Remove(conta);
        _context.SaveChanges();
        return Result.Ok();
    }

    public Result<ReadContaDto> Buscar(int id, string idUsuario, bool eAdmin)
    {
        Conta? conta;
        if (eAdmin)
        {
            conta = _context.Contas.FirstOrDefault(c => c.Numero.Equals(id));
        }
        else
        {
            conta = _context.Contas.FirstOrDefault(c => c.Numero.Equals(id) && c.IdUsuario == idUsuario);
        }
        if (conta is null) return Result.Fail(new NaoEncontradoError("Conta não encontrada"));;
        var readDto = _mapper.Map<ReadContaDto>(conta);
        return Result.Ok(readDto);
    }
}