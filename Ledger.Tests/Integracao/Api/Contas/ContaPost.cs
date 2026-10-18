using System.Net;
using System.Net.Http.Json;
using Ledger.Data.Dtos.ContaDtos;
using Ledger.Tests.Integracao.Api.Suporte;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ledger.Tests.Integracao.Api.Contas;

[Collection(nameof(ApiCollection))]
public class ContaPost : IAsyncLifetime
{
    private readonly LedgerWebApplicationFactory _app;

    public ContaPost(LedgerWebApplicationFactory app)
    {
        _app = app;
    }

    public async ValueTask InitializeAsync() => await _app.LimparBancoAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task CriaContaParaOUsuarioLogado()
    {
        using var client = await _app.CriarClienteAutenticadoAsync("joao@teste.com");

        var resposta = await client.PostAsJsonAsync("/Conta", new { nome = "Conta corrente", cpf = "52998224725" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var conta = await resposta.Content.ReadFromJsonAsync<ReadContaDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(conta);
        Assert.True(conta.Numero > 0);
        Assert.Equal("Conta corrente", conta.Nome);
        Assert.Equal(0m, conta.Saldo);

        await using var contexto = _app.CriarContexto();
        var contaSalva = await contexto.Contas.SingleAsync(c => c.Numero == conta.Numero, TestContext.Current.CancellationToken);
        var dono = await contexto.Users.SingleAsync(u => u.Email == "joao@teste.com", TestContext.Current.CancellationToken);
        Assert.Equal(dono.Id, contaSalva.IdUsuario);
    }

    [Fact]
    public async Task CpfInvalidoDevolve400ComOErroNoCampo()
    {
        using var client = await _app.CriarClienteAutenticadoAsync("joao@teste.com");

        var resposta = await client.PostAsJsonAsync("/Conta", new { nome = "Conta corrente", cpf = "12345678900" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        var problema = await resposta.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestContext.Current.CancellationToken);
        Assert.NotNull(problema);
        Assert.True(problema.Errors.ContainsKey("Cpf"));
    }
}
