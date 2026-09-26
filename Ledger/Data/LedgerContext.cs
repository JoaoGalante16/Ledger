using Ledger.Models;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Ledger.Data
{
    public class LedgerContext : IdentityDbContext<Usuario>
    {
        public LedgerContext(DbContextOptions<LedgerContext> options) : base(options)
        {
        }

        public DbSet<Conta> Contas { get; set; }
        public DbSet<Lancamento> Lancamentos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Lancamento>()
                .HasOne(l => l.Conta)
                .WithMany()
                .HasForeignKey(l => l.NumeroConta)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Conta>()
                .HasOne(c => c.Usuario)
                .WithMany()
                .HasForeignKey(c => c.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.HasSequence<int>("TransacaoIdSeq");

            modelBuilder.Entity<Lancamento>()
                .Property(l => l.IdTransacao)
                .HasDefaultValueSql("nextval('\"TransacaoIdSeq\"')");
        }
    }
}
