using Ledger.Domain.Repositories;
using Ledger.Infrastructure.Data;

namespace Ledger.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly LedgerContext _context;
    
    private ContaRepository _contaRepository;
    private LancamentoRepository _lancamentoRepository;

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
    
    public async Task CommitAsync()
    {
        await _context.SaveChangesAsync();
    }
}