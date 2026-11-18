using System.Linq.Expressions;
using Ledger.Domain.Entities;

namespace Ledger.Domain.Repositories;

public interface ILancamentoRepository
{
    Task<Pagina<Lancamento>> BuscarPaginaAsync(int pagina, int tamanhoPagina, string? idUsuario = null);
    Task<Lancamento?> BuscarPorIdAsync(Expression<Func<Lancamento, bool>> predicate);
    Task AdicionarAsync(Lancamento lancamento);
    Task<int> ProximoIdTransacaoAsync();
    Task<bool> ExisteLancamentoNaContaAsync(int numeroConta);
    Task<List<Lancamento>> BuscarPorTransacaoAsync(int idTransacao);
    Task<decimal> SomarSaldoAsync(int numeroConta);
    Task<Dictionary<int, decimal>> SomarSaldosAsync(List<int> numerosContas);
}