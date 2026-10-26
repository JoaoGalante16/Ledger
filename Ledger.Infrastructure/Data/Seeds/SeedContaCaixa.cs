using Ledger.Domain.Constantes;
using Ledger.Domain.Entities;
using Ledger.Domain.ValueObjects;
using Ledger.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace Ledger.Infrastructure.Data.Seeds;

public class SeedContaCaixa
{
    private readonly LedgerContext _context;

    public SeedContaCaixa(LedgerContext context)
    {
        _context = context;
    }

    public async Task CriarAsync()
    {
        if (!await _context.Users.AnyAsync(u => u.Id == ContaCaixa.IdUsuario))
        {
            _context.Users.Add(new Usuario
            {
                Id = ContaCaixa.IdUsuario,
                UserName = ContaCaixa.Email,
                NormalizedUserName = ContaCaixa.Email.ToUpperInvariant(),
                Email = ContaCaixa.Email,
                NormalizedEmail = ContaCaixa.Email.ToUpperInvariant(),
                SecurityStamp = Guid.NewGuid().ToString()
            });
        }

        if (!await _context.Contas.AnyAsync(c => c.IdUsuario == ContaCaixa.IdUsuario))
        {
            _context.Contas.Add(new Conta
            {
                Nome = ContaCaixa.Nome,
                Cpf = new Cpf(ContaCaixa.Cpf),
                IdUsuario = ContaCaixa.IdUsuario
            });
        }

        await _context.SaveChangesAsync();
    }
}