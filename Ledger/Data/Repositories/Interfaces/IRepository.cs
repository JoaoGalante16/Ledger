using System.Linq.Expressions;

namespace Ledger.Data.Repositories.Interfaces;

public interface IRepository<T>
{
    Task<IQueryable<T>> BuscarTodos();
    Task<T?> BuscarPorPk(Expression<Func<T, bool>> predicate);
    Task Adicionar(T entity);
    Task Atualizar(T entity);
    Task Deletar(T entity);

}