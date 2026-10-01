using System.Linq.Expressions;
using Ledger.Data.Repositories.Interfaces;
using Ledger.Models;
using Microsoft.EntityFrameworkCore;

namespace Ledger.Data.Repositories;

public class LancamentoRepository : ILancamentoRepository
{
    private  readonly LedgerContext _context;
    
    public LancamentoRepository(LedgerContext context)
    {
        _context = context;
    }

    public async Task<IQueryable<Lancamento>> BuscarTodos()
    {
        return _context.Lancamentos.AsNoTracking();
    }

    public Task<Lancamento?> BuscarPorId(Expression<Func<Lancamento, bool>> predicate)
    {
        return _context.Lancamentos.SingleOrDefaultAsync(predicate);
    }

    public async Task Adicionar(Lancamento lancamento)
    {
        await _context.Lancamentos.AddAsync(lancamento);
    }
}