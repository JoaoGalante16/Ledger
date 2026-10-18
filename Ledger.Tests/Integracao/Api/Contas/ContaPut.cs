using System.Net;
using System.Net.Http.Json;
using Ledger.Data.Dtos.ContaDtos;
using Ledger.Tests.Integracao.Api.Suporte;

namespace Ledger.Tests.Integracao.Api.Contas;

[Collection(nameof(ApiCollection))]
public class ContaPut : IAsyncLifetime
{
    private readonly LedgerWebApplicationFactory _app;

    public ContaPut(LedgerWebApplicationFactory app)
    {
        _app = app;
    }

    public async ValueTask InitializeAsync() => await _app.LimparBancoAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task AtualizaONomeDaConta()
    {
        using var client = await _app.CriarClienteAutenticadoAsync("joao@teste.com");
        var conta = await _app.CriarContaAsync(client);

        var resposta = await client.PutAsJsonAsync($"/Conta/{conta.Numero}", new { nome = "Conta da viagem" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        var contaAtualizada = await client.GetFromJsonAsync<ReadContaDto>($"/Conta/{conta.Numero}", TestContext.Current.CancellationToken);
        Assert.Equal("Conta da viagem", contaAtualizada!.Nome);
    }

    [Fact]
    public async Task NaoAtualizaContaDeOutroUsuario()
    {
        using var joao = await _app.CriarClienteAutenticadoAsync("joao@teste.com");
        using var maria = await _app.CriarClienteAutenticadoAsync("maria@teste.com");
        var contaDaMaria = await _app.CriarContaAsync(maria, "Conta da Maria");

        var resposta = await joao.PutAsJsonAsync($"/Conta/{contaDaMaria.Numero}", new { nome = "Invadida" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        var conta = await maria.GetFromJsonAsync<ReadContaDto>($"/Conta/{contaDaMaria.Numero}", TestContext.Current.CancellationToken);
        Assert.Equal("Conta da Maria", conta!.Nome);
    }
}
