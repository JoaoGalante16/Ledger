using Ledger.Data;
using Ledger.Data.ContaDtos;
using Ledger.Models;

namespace Ledger.Services;

public class ContaService : IContaService
{
    private readonly LedgerContext _context;

    public ContaService(LedgerContext context)
    {
        _context = context;
    }

    public ReadContaDto Criar(CreateContaDto dto)
    {
        var conta = new Conta()
        {
            Nome = dto.Nome,
            Cpf = dto.Cpf
        };
        _context.Contas.Add(conta);
        _context.SaveChanges();
        return new ReadContaDto()
        {
            Numero = conta.Numero,
            Nome = conta.Nome,
            Cpf = conta.Cpf
        };
    }

    public List<ReadContaDto> Listar()
    {
        return _context.Contas.Select(c => new ReadContaDto()
            {
                Numero = c.Numero,
                Nome = c.Nome,
                Cpf = c.Cpf
            })
            .ToList();
    }

    public bool Atualizar(UpdateContaDto dto, int id)
    {
        var conta = _context.Contas.FirstOrDefault(c =>  c.Numero.Equals(id));
        if  (conta is null) return false; 

        conta.Nome = dto.Nome;
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
        return new ReadContaDto()
        {
            Numero = conta.Numero,
            Nome = conta.Nome,
            Cpf = conta.Cpf
        };
    }
}