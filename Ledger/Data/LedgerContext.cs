using Ledger.Models;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace Ledger.Data
{
    public class LedgerContext : DbContext
    {
        public LedgerContext(DbContextOptions<LedgerContext> options) : base(options)
        {
        }

        public DbSet<Conta> Contas { get; set; }
        public DbSet<Lancamento> Lancamentos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Lancamento>()
                .HasOne(l => l.Conta)
                .WithMany()
                .HasForeignKey(l => l.NumeroConta)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
