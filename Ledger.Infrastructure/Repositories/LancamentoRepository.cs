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

    public async Task<List<Lancamento>> BuscarTodos()
    {
        return await _context.Lancamentos.ToListAsync();
    }

    public Task<Lancamento?> BuscarPorId(Expression<Func<Lancamento, bool>> predicate)
    {
        return _context.Lancamentos.SingleOrDefaultAsync(predicate);
    }

    public async Task Adicionar(Lancamento lancamento)
    {
        await _context.Lancamentos.AddAsync(lancamento);
    }

    public async Task<int> ProximoIdTransacao()
    {
        return await _context.Database
            .SqlQuery<int>($"SELECT nextval('\"TransacaoIdSeq\"') as \"Value\"")
            .FirstAsync();
    }

    public async Task<bool> ExisteLancamentoNaConta(int numeroConta)
    {
       return await _context.Lancamentos.AnyAsync(l => l.NumeroConta == numeroConta);
    }
    
    public async Task<List<Lancamento>> BuscarPorUsuario(string idUsuario)
    {
        return await _context.Lancamentos.Where(l => l.Conta.IdUsuario == idUsuario).ToListAsync();
    }

    public Task<List<Lancamento>> BuscarPorTransacao(int idTransacao)
    {
        return _context.Lancamentos.Where(l => l.IdTransacao == idTransacao).OrderBy(l => l.Valor).ToListAsync();
    }

    public Task<decimal> SomarSaldo(int numeroConta)
    {
        return _context.Lancamentos.Where(l => l.NumeroConta == numeroConta).SumAsync(l => l.Valor);
    }

    public Task<Dictionary<int, decimal>> SomarSaldos(List<int> numerosContas)
    {
        return _context.Lancamentos.Where(l => numerosContas.Contains(l.NumeroConta)).GroupBy(l => l.NumeroConta).Select(g => new { NumeroConta = g.Key, Saldo = g.Sum(l => l.Valor) }).ToDictionaryAsync(x => x.NumeroConta, x => x.Saldo);
    }
}