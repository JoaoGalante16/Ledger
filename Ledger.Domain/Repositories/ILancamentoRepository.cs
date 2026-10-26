using System.Linq.Expressions;
using Ledger.Domain.Entities;

namespace Ledger.Domain.Repositories;

public interface ILancamentoRepository
{
    Task<List<Lancamento>> BuscarTodosAsync();
    Task<Lancamento?> BuscarPorIdAsync(Expression<Func<Lancamento, bool>> predicate);
    Task AdicionarAsync(Lancamento lancamento);
    Task<int> ProximoIdTransacaoAsync();
    Task<bool> ExisteLancamentoNaContaAsync(int numeroConta);
    Task<List<Lancamento>> BuscarPorUsuarioAsync(string idUsuario);
    Task<List<Lancamento>> BuscarPorTransacaoAsync(int idTransacao);
    Task<decimal> SomarSaldoAsync(int numeroConta);
    Task<Dictionary<int, decimal>> SomarSaldosAsync(List<int> numerosContas);
}