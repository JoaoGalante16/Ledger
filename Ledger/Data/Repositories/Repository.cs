using System.Linq.Expressions;
using Ledger.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Ledger.Data.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    private LedgerContext  _context;
    public Repository(LedgerContext context)
    {
        _context = context;
    }
    
    public async Task<IQueryable<T>> BuscarTodos()
    {
        return _context.Set<T>().AsNoTracking();
    }

    public Task<T?> BuscarPorPk(Expression<Func<T, bool>> predicate)
    {
        return _context.Set<T>().SingleOrDefaultAsync(predicate);
    }

    public async Task Adicionar(T entity)
    {
        await _context.Set<T>().AddAsync(entity);
    }

    public async Task Atualizar(T entity)
    {
        _context.Set<T>().Update(entity);
    }

    public async Task Deletar(T entity)
    {
        _context.Set<T>().Remove(entity);
    }
}