using Ledger.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Ledger.Tests.Integracao.BancoDeDados.Suporte;

namespace Ledger.Tests.Integracao.BancoDeDados.Lancamentos;

[Collection(nameof(ContextoCollection))]
public class LancamentoRepositoryBuscarTodos : IAsyncLifetime
{
    private readonly ContextoFixture _fixture;

    public LancamentoRepositoryBuscarTodos(ContextoFixture fixture)
    {
        _fixture = fixture;
    }

    public async ValueTask InitializeAsync() => await _fixture.LimparBancoAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task RetornaTodosOsLancamentos()
    {
        var conta = await _fixture.CriarContaAsync(await _fixture.CriarUsuarioAsync());
        await _fixture.CriarLancamentoAsync(conta, 10m);
        await _fixture.CriarLancamentoAsync(conta, 20m);
        await _fixture.CriarLancamentoAsync(conta, -5m);
        await using var contexto = _fixture.CriarContexto();
        var repositorio = new LancamentoRepository(contexto);
        
        var lancamentos = await (await repositorio.BuscarTodos()).ToListAsync(TestContext.Current.CancellationToken);
        
        Assert.Equal(3, lancamentos.Count);
    }

    [Fact]
    public async Task SomaOSaldoDeUmaContaNoBanco()
    {
        var conta = await _fixture.CriarContaAsync(await _fixture.CriarUsuarioAsync());
        var outraConta = await _fixture.CriarContaAsync(await _fixture.CriarUsuarioAsync());
        await _fixture.CriarLancamentoAsync(conta, 0.10m);
        await _fixture.CriarLancamentoAsync(conta, 0.20m);
        await _fixture.CriarLancamentoAsync(outraConta, 1000m);
        await using var contexto = _fixture.CriarContexto();
        var repositorio = new LancamentoRepository(contexto);
        
        var saldo = await (await repositorio.BuscarTodos())
            .Where(l => l.NumeroConta == conta.Numero)
            .SumAsync(l => l.Valor, TestContext.Current.CancellationToken);
        
        Assert.Equal(0.30m, saldo);
    }

    [Fact]
    public async Task SomaZeroQuandoAContaNaoTemLancamentos()
    {
        var conta = await _fixture.CriarContaAsync(await _fixture.CriarUsuarioAsync());
        await using var contexto = _fixture.CriarContexto();
        var repositorio = new LancamentoRepository(contexto);
        
        var saldo = await (await repositorio.BuscarTodos())
            .Where(l => l.NumeroConta == conta.Numero)
            .SumAsync(l => l.Valor, TestContext.Current.CancellationToken);

        Assert.Equal(0m, saldo);
    }
}
