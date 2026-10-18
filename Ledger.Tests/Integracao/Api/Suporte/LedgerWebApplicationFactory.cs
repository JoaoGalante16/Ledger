using Ledger.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace Ledger.Tests.Integracao.Api.Suporte;

public class LedgerWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder("postgres:16").Build();
    private Respawner _respawner = null!;

    public async ValueTask InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        
        await using var contexto = CriarContexto();
        await contexto.Database.MigrateAsync();

        await using var conexao = new NpgsqlConnection(_postgresContainer.GetConnectionString());
        await conexao.OpenAsync();
        _respawner = await Respawner.CreateAsync(conexao, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            TablesToIgnore = ["__EFMigrationsHistory", "AspNetRoles"]
        });
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<LedgerContext>));
            services.AddDbContext<LedgerContext>(options =>
                options
                    .UseLazyLoadingProxies()
                    .UseNpgsql(_postgresContainer.GetConnectionString()));
        });

        base.ConfigureWebHost(builder);
    }

    public LedgerContext CriarContexto()
    {
        var options = new DbContextOptionsBuilder<LedgerContext>()
            .UseLazyLoadingProxies()
            .UseNpgsql(_postgresContainer.GetConnectionString())
            .Options;

        return new LedgerContext(options);
    }

    public async Task LimparBancoAsync()
    {
        await using var conexao = new NpgsqlConnection(_postgresContainer.GetConnectionString());
        await conexao.OpenAsync();
        await _respawner.ResetAsync(conexao);
    }

    public override async ValueTask DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}
