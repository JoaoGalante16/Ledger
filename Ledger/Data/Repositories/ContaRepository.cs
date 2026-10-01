using Ledger.Data.Repositories.Interfaces;
using Ledger.Models;

namespace Ledger.Data.Repositories;

public class ContaRepository : Repository<Conta>, IContaRepository
{
    public ContaRepository(LedgerContext context) : base(context)
    {
    }
}