using Ledger.Domain.Constantes;
using Ledger.Domain.Entities;
using Ledger.Domain.Repositories;
using Ledger.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ledger.Infrastructure.Repositories;

public class ContaRepository : Repository<Conta>, IContaRepository
{
    public ContaRepository(LedgerContext context) : base(context)
    {
        
    }

    public async Task<List<Conta>> BuscarPorUsuarioAsync(string idUsuario)
    {
        return await _context.Contas.Where(c => c.IdUsuario == idUsuario).ToListAsync();
    }

    public async Task<Conta> BuscarContaCaixaAsync()
    {
        return await _context.Contas.SingleAsync(c => c.IdUsuario == ContaCaixa.IdUsuario);
    }
}