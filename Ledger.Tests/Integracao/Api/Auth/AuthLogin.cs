using System.Net;
using System.Net.Http.Json;
using Ledger.Tests.Integracao.Api.Suporte;
using Microsoft.AspNetCore.Authentication.BearerToken;

namespace Ledger.Tests.Integracao.Api.Auth;

[Collection(nameof(ApiCollection))]
public class AuthLogin : IAsyncLifetime
{
    private readonly LedgerWebApplicationFactory _app;

    public AuthLogin(LedgerWebApplicationFactory app)
    {
        _app = app;
    }

    public async ValueTask InitializeAsync() => await _app.LimparBancoAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private async Task<HttpClient> ClienteComUsuarioRegistradoAsync()
    {
        var client = _app.CreateClient();
        await client.PostAsJsonAsync("/auth/register",
            new { email = "joao@teste.com", password = LedgerWebApplicationFactory.SenhaPadrao },
            TestContext.Current.CancellationToken);
        return client;
    }

    [Fact]
    public async Task LoginComSenhaCertaDevolveToken()
    {
        using var client = await ClienteComUsuarioRegistradoAsync();

        var resposta = await client.PostAsJsonAsync("/auth/login",
            new { email = "joao@teste.com", password = LedgerWebApplicationFactory.SenhaPadrao },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var token = await resposta.Content.ReadFromJsonAsync<AccessTokenResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(token);
        Assert.Equal("Bearer", token.TokenType);
        Assert.False(string.IsNullOrWhiteSpace(token.AccessToken));
    }

    [Fact]
    public async Task LoginComSenhaErradaDevolve401()
    {
        using var client = await ClienteComUsuarioRegistradoAsync();

        var resposta = await client.PostAsJsonAsync("/auth/login",
            new { email = "joao@teste.com", password = "SenhaErrada@1" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task LoginDeUsuarioInexistenteDevolve401()
    {
        using var client = _app.CreateClient();

        var resposta = await client.PostAsJsonAsync("/auth/login",
            new { email = "ninguem@teste.com", password = LedgerWebApplicationFactory.SenhaPadrao },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task EndpointProtegidoSemTokenDevolve401()
    {
        using var client = _app.CreateClient();
        
        var resposta = await client.GetAsync("/Conta", TestContext.Current.CancellationToken);
        
        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task EndpointProtegidoComTokenDevolve200()
    {
        using var client = await _app.CriarClienteAutenticadoAsync("joao@teste.com");
        
        var resposta = await client.GetAsync("/Conta", TestContext.Current.CancellationToken);
        
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }
}
