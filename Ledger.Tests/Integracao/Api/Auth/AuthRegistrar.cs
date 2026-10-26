using System.Net;
using System.Net.Http.Json;
using Ledger.Tests.Integracao.Api.Suporte;
using Microsoft.EntityFrameworkCore;

namespace Ledger.Tests.Integracao.Api.Auth;

[Collection(nameof(ApiCollection))]
public class AuthRegistrar : IAsyncLifetime
{
    private readonly LedgerWebApplicationFactory _app;

    public AuthRegistrar(LedgerWebApplicationFactory app)
    {
        _app = app;
    }

    public async ValueTask InitializeAsync() => await _app.LimparBancoAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task RegistraUsuarioNovo()
    {
        using var client = _app.CreateClient();

        var resposta = await client.PostAsJsonAsync("/auth/register",
            new { email = "joao@teste.com", password = LedgerWebApplicationFactory.SenhaPadrao },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        await using var contexto = _app.CriarContexto();
        Assert.True(await contexto.Users.AnyAsync(u => u.Email == "joao@teste.com", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task NaoRegistraEmailRepetido()
    {
        using var client = _app.CreateClient();
        var usuario = new { email = "joao@teste.com", password = LedgerWebApplicationFactory.SenhaPadrao };
        await client.PostAsJsonAsync("/auth/register", usuario, TestContext.Current.CancellationToken);

        var resposta = await client.PostAsJsonAsync("/auth/register", usuario, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task NaoRegistraSenhaFraca()
    {
        using var client = _app.CreateClient();

        var resposta = await client.PostAsJsonAsync("/auth/register",
            new { email = "joao@teste.com", password = "123" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        await using var contexto = _app.CriarContexto();
        Assert.False(await contexto.Users.AnyAsync(u => u.Email == "joao@teste.com", TestContext.Current.CancellationToken));
    }
}
