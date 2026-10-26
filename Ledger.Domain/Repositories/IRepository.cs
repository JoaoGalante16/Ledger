using System.Linq.Expressions;

namespace Ledger.Domain.Repositories;

public interface IRepository<T>
{
    Task<List<T>> BuscarTodosAsync();
    Task<T?> BuscarPorPkAsync(Expression<Func<T, bool>> predicate);
    Task AdicionarAsync(T entity);
    Task AtualizarAsync(T entity);
    Task DeletarAsync(T entity);

}