using Bogus;
using Bogus.Extensions.Brazil;
using Ledger.Domain.Entities;
using Ledger.Infrastructure;
using Ledger.Infrastructure.Data;
using Ledger.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace Ledger.Tests.Integracao.BancoDeDados.Suporte;

public class ContextoFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder("postgres:16").Build();
    private readonly Faker _faker = new("pt_BR");
    private Respawner _respawner = null!;

    public async ValueTask InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        await using var contexto = CriarContexto();
        await contexto.Database.MigrateAsync();

        await using var conexao = new NpgsqlConnection(_postgresContainer.GetConnectionString());
        await conexao.OpenAsync();
        
        // cria script de limpeza que eh chamado com ResetAsync
        _respawner = await Respawner.CreateAsync(conexao, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            TablesToIgnore = ["__EFMigrationsHistory"]
        });
    }
    
    public LedgerContext CriarContexto()
    {
        var options = new DbContextOptionsBuilder<LedgerContext>()
            .UseLazyLoadingProxies()
            .UseNpgsql(_postgresContainer.GetConnectionString())
            .Options;

        return new LedgerContext(options);
    }

    
    // executa o script de limpeza do respawner
    public async Task LimparBancoAsync()
    {
        await using var conexao = new NpgsqlConnection(_postgresContainer.GetConnectionString());
        await conexao.OpenAsync();
        await _respawner.ResetAsync(conexao);
    }

    public async Task<Usuario> CriarUsuarioAsync()
    {
        var email = _faker.Internet.Email();
        var usuario = new Usuario { UserName = email, Email = email };

        await using var contexto = CriarContexto();
        contexto.Users.Add(usuario);
        await contexto.SaveChangesAsync();
        return usuario;
    }

    public async Task<Conta> CriarContaAsync(Usuario dono)
    {
        var conta = NovaConta(dono);

        await using var contexto = CriarContexto();
        contexto.Contas.Add(conta);
        await contexto.SaveChangesAsync();
        return conta;
    }

    public async Task<Lancamento> CriarLancamentoAsync(Conta conta, decimal valor, int idTransacao = 0)
    {
        var lancamento = NovoLancamento(conta, valor);
        lancamento.IdTransacao = idTransacao;

        await using var contexto = CriarContexto();
        contexto.Lancamentos.Add(lancamento);
        await contexto.SaveChangesAsync();
        return lancamento;
    }

    public Conta NovaConta(Usuario dono)
    {
        var pessoa = new Person("pt_BR");
        return new Conta
        {
            Nome = pessoa.FullName,
            Cpf = pessoa.Cpf(includeFormatSymbols: false),
            IdUsuario = dono.Id
        };
    }

    public Lancamento NovoLancamento(Conta conta, decimal valor) => new()
    {
        NumeroConta = conta.Numero,
        Valor = valor,
        DataTransacao = DateTime.UtcNow,
        DataGravacao = DateTime.UtcNow,
        Descricao = _faker.Lorem.Sentence()
    };

    public async ValueTask DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
    }
}
