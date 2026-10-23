using Ledger.Domain.Entities;
using Ledger.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Ledger.Infrastructure.Data
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

            // ---- Conta ----
            modelBuilder.Entity<Conta>()
                .HasKey(c => c.Numero);

            modelBuilder.Entity<Conta>()
                .Property(c => c.Nome).IsRequired();

            modelBuilder.Entity<Conta>()
                .Property(c => c.Cpf).IsRequired();

            modelBuilder.Entity<Conta>()
                .HasOne<Usuario>() // FK pelo IdUsuario, o Domain nao conhece o Usuario
                .WithMany()
                .HasForeignKey(c => c.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            // ---- Lancamento ----
            modelBuilder.Entity<Lancamento>()
                .HasKey(l => l.Id);

            modelBuilder.Entity<Lancamento>()
                .Property(l => l.Valor)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Lancamento>()
                .HasOne(l => l.Conta)
                .WithMany()
                .HasForeignKey(l => l.NumeroConta)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Lancamento>()
                .HasOne(l => l.LancamentoReferencia)
                .WithMany()
                .HasForeignKey(l => l.IdLancamentoReferencia);

            modelBuilder.HasSequence<int>("TransacaoIdSeq");

            modelBuilder.Entity<Lancamento>()
                .Property(l => l.IdTransacao)
                .HasDefaultValueSql("nextval('\"TransacaoIdSeq\"')");
        }
    }
}
