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

    public async Task<Pagina<Conta>> BuscarPaginaAsync(int pagina, int tamanhoPagina, string? idUsuario = null)
    {
        var consulta = _context.Contas.AsQueryable();
        if (idUsuario is not null)
            consulta = consulta.Where(c => c.IdUsuario == idUsuario);

        var totalItens = await consulta.CountAsync();
        var itens = await consulta
            .OrderBy(c => c.Numero)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync();

        return new Pagina<Conta>(itens, totalItens);
    }

    public async Task<Conta> BuscarContaCaixaAsync()
    {
        return await _context.Contas.SingleAsync(c => c.IdUsuario == ContaCaixa.IdUsuario);
    }

    public Task<int> BuscarNumeroContaCaixaAsync()
    {
        return _context.Contas
            .Where(c => c.IdUsuario == ContaCaixa.IdUsuario)
            .Select(c => c.Numero)
            .SingleAsync();
    }

    public async Task<List<Conta>> BloquearEBuscarAsync(params int[] numeros)
    {
        // FOR UPDATE trava as linhas até o fim da transacao, ordena por numero para travar as contas sempre na mesma ordem
        await _context.Database.ExecuteSqlAsync(
            $"SELECT 1 FROM \"Contas\" WHERE \"Numero\" = ANY({numeros}) ORDER BY \"Numero\" FOR UPDATE"); 
        
        return await _context.Contas
            .Where(c => numeros.Contains(c.Numero))
            .OrderBy(c => c.Numero)
            .ToListAsync();
    }
}