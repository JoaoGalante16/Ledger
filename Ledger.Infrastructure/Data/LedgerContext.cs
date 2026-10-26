using Ledger.Domain.Entities;
using Ledger.Domain.ValueObjects;
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

            // liga o valueObject a coluna existente e torna obrigatório o Value (string)
            modelBuilder.Entity<Conta>()
                .OwnsOne(c => c.Cpf, cpf =>
                {
                    cpf.Property(p => p.Value)
                        .HasColumnName("Cpf")
                        .IsRequired();
                });

            // torna obrigatório o Cpf
            modelBuilder.Entity<Conta>()
                .Navigation(c => c.Cpf)
                .IsRequired();

            modelBuilder.Entity<Conta>()
                .HasOne<Usuario>() // FK pelo IdUsuario, o Domain nao conhece o Usuario
                .WithMany()
                .HasForeignKey(c => c.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            // ---- Lancamento ----
            modelBuilder.Entity<Lancamento>()
                .HasKey(l => l.Id);

            modelBuilder.Entity<Lancamento>()
                .OwnsOne(l => l.Valor, valor =>
                {
                    valor.Property(p => p.Value)
                        .HasColumnName("Valor")
                        .HasColumnType("decimal(18,2)")
                        .IsRequired();
                });

            modelBuilder.Entity<Lancamento>()
                .Navigation(l => l.Valor)
                .IsRequired();

            modelBuilder.Entity<Lancamento>()
                .Property(l => l.TipoOperacao)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

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
