using System.Net;
using System.Net.Http.Json;
using Ledger.Data.Dtos.ContaDtos;
using Ledger.Tests.Integracao.Api.Suporte;
using Microsoft.AspNetCore.Mvc;

namespace Ledger.Tests.Integracao.Api.Contas;

[Collection(nameof(ApiCollection))]
public class ContaGet : IAsyncLifetime
{
    private readonly LedgerWebApplicationFactory _app;

    public ContaGet(LedgerWebApplicationFactory app)
    {
        _app = app;
    }

    public async ValueTask InitializeAsync() => await _app.LimparBancoAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task ListaSoAsContasDoUsuarioLogado()
    {
        using var joao = await _app.CriarClienteAutenticadoAsync("joao@teste.com");
        using var maria = await _app.CriarClienteAutenticadoAsync("maria@teste.com");
        var contaDoJoao = await _app.CriarContaAsync(joao);
        await _app.CriarContaAsync(maria);

        var contas = await joao.GetFromJsonAsync<List<ReadContaDto>>("/Conta", TestContext.Current.CancellationToken);

        Assert.NotNull(contas);
        var conta = Assert.Single(contas);
        Assert.Equal(contaDoJoao.Numero, conta.Numero);
    }

    [Fact]
    public async Task BuscaContaComOSaldoCalculado()
    {
        using var client = await _app.CriarClienteAutenticadoAsync("joao@teste.com");
        var conta = await _app.CriarContaAsync(client);
        await _app.DepositarAsync(conta.Numero, 150.75m);

        var resposta = await client.GetFromJsonAsync<ReadContaDto>($"/Conta/{conta.Numero}", TestContext.Current.CancellationToken);

        Assert.NotNull(resposta);
        Assert.Equal(150.75m, resposta.Saldo);
    }

    [Fact]
    public async Task BuscaContaDeOutroUsuarioDevolve404ComProblemDetails()
    {
        using var joao = await _app.CriarClienteAutenticadoAsync("joao@teste.com");
        using var maria = await _app.CriarClienteAutenticadoAsync("maria@teste.com");
        var contaDaMaria = await _app.CriarContaAsync(maria);

        var resposta = await joao.GetAsync($"/Conta/{contaDaMaria.Numero}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        var problema = await resposta.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);
        Assert.NotNull(problema);
        Assert.Equal(404, problema.Status);
        Assert.Equal("Conta não encontrada", problema.Detail);
    }
}
