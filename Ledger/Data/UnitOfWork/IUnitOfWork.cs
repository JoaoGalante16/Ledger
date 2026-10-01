using Ledger.Data.Repositories.Interfaces;

namespace Ledger.Data.UnitOfWork;

public interface IUnitOfWork
{
    IContaRepository ContaRepository { get; }
    ILancamentoRepository LancamentoRepository { get; }
    
    LedgerContext Contexto { get;}

    Task Commit();
}