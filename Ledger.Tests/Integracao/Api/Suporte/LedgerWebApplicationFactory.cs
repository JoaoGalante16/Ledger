using System.Net.Http.Headers;
using System.Net.Http.Json;
using Ledger.Application.Dtos.Contas;
using Ledger.Domain.Entities;
using Ledger.Domain.ValueObjects;
using Ledger.Infrastructure;
using Ledger.Infrastructure.Data;
using Ledger.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
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
    
    public const string SenhaPadrao = "Senha!123";

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
    
    public async Task<HttpClient> CriarClienteAutenticadoAsync(string email, string senha = SenhaPadrao)
    {
        var client = CreateClient();
        await RegistrarAsync(client, email, senha);
        await LogarAsync(client, email, senha);
        return client;
    }

    public async Task<HttpClient> CriarClienteAdminAsync(string email, string senha = SenhaPadrao)
    {
        var client = CreateClient();
        await RegistrarAsync(client, email, senha);
        
        using var scope = Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();
        var usuario = await userManager.FindByEmailAsync(email);
        await userManager.AddToRoleAsync(usuario!, "Admin");

        await LogarAsync(client, email, senha);
        return client;
    }

    public async Task<ReadContaDto> CriarContaAsync(HttpClient client, string nome = "Conta corrente")
    {
        var resposta = await client.PostAsJsonAsync("/Conta", new { nome, cpf = "52998224725" },
            TestContext.Current.CancellationToken);
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<ReadContaDto>(TestContext.Current.CancellationToken))!;
    }
    
    public async Task<Lancamento> DepositarAsync(int numeroConta, decimal valor)
    {
        var deposito = new Lancamento
        {
            NumeroConta = numeroConta,
            Valor = new Valor(valor),
            DataTransacao = DateTime.UtcNow,
            DataGravacao = DateTime.UtcNow,
            Descricao = "Depósito inicial"
        };

        await using var contexto = CriarContexto();
        contexto.Lancamentos.Add(deposito);
        await contexto.SaveChangesAsync(TestContext.Current.CancellationToken);
        return deposito;
    }

    private static async Task RegistrarAsync(HttpClient client, string email, string senha)
    {
        var registro = await client.PostAsJsonAsync("/auth/register", new { email, password = senha },
            TestContext.Current.CancellationToken);
        registro.EnsureSuccessStatusCode();
    }

    private static async Task LogarAsync(HttpClient client, string email, string senha)
    {
        var cancelamento = TestContext.Current.CancellationToken;
        var login = await client.PostAsJsonAsync("/auth/login", new { email, password = senha }, cancelamento);
        login.EnsureSuccessStatusCode();
        var resposta = await login.Content.ReadFromJsonAsync<AccessTokenResponse>(cancelamento);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", resposta!.AccessToken);
    }

    public override async ValueTask DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}
