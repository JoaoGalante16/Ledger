namespace Ledger.Domain.Repositories;

public interface IUnitOfWork
{
    IContaRepository ContaRepository { get; }
    ILancamentoRepository LancamentoRepository { get; }

    Task CommitAsync();
}