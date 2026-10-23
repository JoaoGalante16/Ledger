using System.Linq.Expressions;
using Ledger.Domain.Entities;

namespace Ledger.Domain.Repositories;

public interface ILancamentoRepository
{
    Task<List<Lancamento>> BuscarTodos();
    Task<Lancamento?> BuscarPorId(Expression<Func<Lancamento, bool>> predicate);
    Task Adicionar(Lancamento lancamento);
    Task<int> ProximoIdTransacao();
    Task<bool> ExisteLancamentoNaConta(int numeroConta);
    Task<List<Lancamento>> BuscarPorUsuario(string idUsuario);
    Task<List<Lancamento>> BuscarPorTransacao(int idTransacao);
    Task<decimal> SomarSaldo(int numeroConta);
    Task<Dictionary<int, decimal>> SomarSaldos(List<int> numerosContas);
}