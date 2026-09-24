using AutoMapper;
using Ledger.Data;
using Ledger.Data.ContaDtos;
using Ledger.Models;

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

    public ReadContaDto Criar(CreateContaDto dto)
    {
        var conta = _mapper.Map<Conta>(dto);
        _context.Contas.Add(conta);
        _context.SaveChanges();
        return _mapper.Map<ReadContaDto>(conta);
    }

    public List<ReadContaDto> Listar()
    {
        return _mapper.Map<List<ReadContaDto>>(_context.Contas.ToList());
    }

    public bool Atualizar(UpdateContaDto dto, int id)
    {
        var conta = _context.Contas.FirstOrDefault(c =>  c.Numero.Equals(id));
        if  (conta is null) return false;
        _mapper.Map(dto, conta);
        _context.SaveChanges();
        return true;
    }

    public bool Remover(int id)
    {
        var conta = _context.Contas.FirstOrDefault(c=>c.Numero.Equals(id));
        if (conta is null) return false;
        _context.Contas.Remove(conta);
        _context.SaveChanges();
        return true;
    }

    public ReadContaDto? Buscar(int id)
    {
        var conta = _context.Contas.FirstOrDefault(c => c.Numero.Equals(id));
        if (conta is null) return null;
        return _mapper.Map<ReadContaDto>(conta);
    }
}