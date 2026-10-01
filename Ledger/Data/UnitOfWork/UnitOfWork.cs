using Ledger.Data.Repositories;
using Ledger.Data.Repositories.Interfaces;

namespace Ledger.Data.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private ContaRepository _contaRepository;
    private LancamentoRepository _lancamentoRepository;

    public IContaRepository ContaRepository
    {
        get
        {
            if (_contaRepository == null)
            {
                _contaRepository = new ContaRepository(Contexto);
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
                _lancamentoRepository = new LancamentoRepository(Contexto);
            }

            return _lancamentoRepository;
        }
    }

    public LedgerContext Contexto { get; }

    public UnitOfWork(LedgerContext context)
    {
        Contexto = context;
    }
    
    public async Task Commit()
    {
        await Contexto.SaveChangesAsync();
    }
}