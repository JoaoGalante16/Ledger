using System.Linq.Expressions;
using Ledger.Domain.Repositories;
using Ledger.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ledger.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected LedgerContext _context;
    public Repository(LedgerContext context)
    {
        _context = context;
    }
    
    public async Task<List<T>> BuscarTodosAsync()
    {
        return await _context.Set<T>().ToListAsync();
    }

    public Task<T?> BuscarPorPkAsync(Expression<Func<T, bool>> predicate)
    {
        return _context.Set<T>().SingleOrDefaultAsync(predicate);
    }

    public async Task AdicionarAsync(T entity)
    {
        await _context.Set<T>().AddAsync(entity);
    }

    public async Task AtualizarAsync(T entity)
    {
        _context.Set<T>().Update(entity);
    }

    public async Task DeletarAsync(T entity)
    {
        _context.Set<T>().Remove(entity);
    }
}