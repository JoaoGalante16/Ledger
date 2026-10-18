using System.Net;
using System.Net.Http.Json;
using Ledger.Data.Dtos.ContaDtos;
using Ledger.Tests.Integracao.Api.Suporte;
using Microsoft.AspNetCore.Mvc;

namespace Ledger.Tests.Integracao.Api.Contas;

[Collection(nameof(ApiCollection))]
public class ContaEncerrar : IAsyncLifetime
{
    private readonly LedgerWebApplicationFactory _app;

    public ContaEncerrar(LedgerWebApplicationFactory app)
    {
        _app = app;
    }

    public async ValueTask InitializeAsync() => await _app.LimparBancoAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task EncerraContaComSaldoZero()
    {
        using var client = await _app.CriarClienteAutenticadoAsync("joao@teste.com");
        var conta = await _app.CriarContaAsync(client);

        var resposta = await client.PostAsync($"/Conta/{conta.Numero}/encerrar", null, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        var contaEncerrada = await client.GetFromJsonAsync<ReadContaDto>($"/Conta/{conta.Numero}", TestContext.Current.CancellationToken);
        Assert.NotNull(contaEncerrada!.DataEncerramento);
    }

    [Fact]
    public async Task ContaComSaldoDevolve409()
    {
        using var client = await _app.CriarClienteAutenticadoAsync("joao@teste.com");
        var conta = await _app.CriarContaAsync(client);
        await _app.DepositarAsync(conta.Numero, 10m);

        var resposta = await client.PostAsync($"/Conta/{conta.Numero}/encerrar", null, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
        var problema = await resposta.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);
        Assert.Equal("O saldo precisa ser zero para encerrar a conta", problema!.Detail);
    }
}
