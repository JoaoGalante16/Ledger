using System.Linq.Expressions;
using Ledger.Domain.Entities;
using Ledger.Domain.Repositories;
using Ledger.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ledger.Infrastructure.Repositories;

public class LancamentoRepository : ILancamentoRepository
{
    private  readonly LedgerContext _context;
    
    public LancamentoRepository(LedgerContext context)
    {
        _context = context;
    }

    public async Task<List<Lancamento>> BuscarTodosAsync()
    {
        return await _context.Lancamentos.ToListAsync();
    }

    public Task<Lancamento?> BuscarPorIdAsync(Expression<Func<Lancamento, bool>> predicate)
    {
        return _context.Lancamentos.SingleOrDefaultAsync(predicate);
    }

    public async Task AdicionarAsync(Lancamento lancamento)
    {
        await _context.Lancamentos.AddAsync(lancamento);
    }

    public async Task<int> ProximoIdTransacaoAsync()
    {
        return await _context.Database
            .SqlQuery<int>($"SELECT nextval('\"TransacaoIdSeq\"') as \"Value\"")
            .FirstAsync();
    }

    public async Task<bool> ExisteLancamentoNaContaAsync(int numeroConta)
    {
       return await _context.Lancamentos.AnyAsync(l => l.NumeroConta == numeroConta);
    }
    
    public async Task<List<Lancamento>> BuscarPorUsuarioAsync(string idUsuario)
    {
        return await _context.Lancamentos.Where(l => l.Conta.IdUsuario == idUsuario).ToListAsync();
    }

    public Task<List<Lancamento>> BuscarPorTransacaoAsync(int idTransacao)
    {
        return _context.Lancamentos.Where(l => l.IdTransacao == idTransacao).OrderBy(l => l.Valor.Value).ToListAsync();
    }

    public Task<decimal> SomarSaldoAsync(int numeroConta)
    {
        return _context.Lancamentos.Where(l => l.NumeroConta == numeroConta).SumAsync(l => l.Valor.Value);
    }

    public Task<Dictionary<int, decimal>> SomarSaldosAsync(List<int> numerosContas)
    {
        return _context.Lancamentos.Where(l => numerosContas.Contains(l.NumeroConta)).GroupBy(l => l.NumeroConta).Select(g => new { NumeroConta = g.Key, Saldo = g.Sum(l => l.Valor.Value) }).ToDictionaryAsync(x => x.NumeroConta, x => x.Saldo);
    }
}