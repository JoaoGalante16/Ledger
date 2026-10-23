using System.Linq.Expressions;

namespace Ledger.Domain.Repositories;

public interface IRepository<T>
{
    Task<List<T>> BuscarTodos();
    Task<T?> BuscarPorPk(Expression<Func<T, bool>> predicate);
    Task Adicionar(T entity);
    Task Atualizar(T entity);
    Task Deletar(T entity);

}