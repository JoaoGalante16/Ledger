using System.Linq.Expressions;
using Ledger.Models;

namespace Ledger.Data.Repositories.Interfaces;

public interface ILancamentoRepository
{
    Task<IQueryable<Lancamento>> BuscarTodos();
    Task<Lancamento?> BuscarPorId(Expression<Func<Lancamento, bool>> predicate);
    Task Adicionar(Lancamento lancamento);
}