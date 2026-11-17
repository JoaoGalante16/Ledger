using Ledger.Domain.Repositories;
using Ledger.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Ledger.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly LedgerContext _context;
    
    private ContaRepository _contaRepository;
    private LancamentoRepository _lancamentoRepository;
    private IDbContextTransaction? _transacao;

    public IContaRepository ContaRepository
    {
        get
        {
            if (_contaRepository == null)
            {
                _contaRepository = new ContaRepository(_context);
            }
            return _contaRepository;
        }
    }

    public ILancamentoRepository LancamentoRepository
    {
        get
        {
            if (_lancamentoRepository == null)
            {
                _lancamentoRepository = new LancamentoRepository(_context);
            }

            return _lancamentoRepository;
        }
    }
    
    public UnitOfWork(LedgerContext context)
    {
        _context = context;
    }
    
    public async Task IniciarTransacaoAsync()
    {
        _transacao = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        await _context.SaveChangesAsync();
        if (_transacao is null) return;

        await _transacao.CommitAsync();
        await _transacao.DisposeAsync();
        _transacao = null;
    }
}