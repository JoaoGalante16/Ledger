using Ledger.Application.Errors;
using Ledger.Application.Services;
using Ledger.Tests.Unitarios.Suporte;

namespace Ledger.Tests.Unitarios.Services;

public class LancamentoServiceConsultas
{
    private readonly CenarioDeServico _cenario = new();
    private readonly LancamentoService _service;

    public LancamentoServiceConsultas()
    {
        _service = new LancamentoService(_cenario.UnitOfWork.Object, _cenario.Mapper);
    }

    // ---- BuscarParDeLancamentos ----

    [Fact]
    public async Task BuscarParDeLancamentosDevolveODebitoPrimeiro()
    {
        var origem = _cenario.NovaConta(1);
        var destino = _cenario.NovaConta(2);
        _cenario.NovoLancamento(destino, 80m, idTransacao: 5);
        _cenario.NovoLancamento(origem, -80m, idTransacao: 5);
        _cenario.NovoLancamento(origem, 999m, idTransacao: 6);

        var resultado = await _service.BuscarParDeLancamentosAsync(5);

        Assert.True(resultado.IsSuccess);
        Assert.Equal(new[] { -80m, 80m }, resultado.Value.Select(l => l.Valor));
        Assert.Equal(new[] { 1, 2 }, resultado.Value.Select(l => l.NumeroConta));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    public async Task BuscarParDeLancamentosRetornaNaoEncontradoQuandoNaoHaExatamenteDoisLancamentos(int quantidade)
    {
        var conta = _cenario.NovaConta(1);
        for (var i = 0; i < quantidade; i++)
            _cenario.NovoLancamento(conta, 10m, idTransacao: 5);

        var resultado = await _service.BuscarParDeLancamentosAsync(5);

        Assert.True(resultado.HasError<NaoEncontradoError>());
    }

    // ---- Buscar ----

    [Theory]
    [InlineData(CenarioDeServico.IdDono, false)]
    [InlineData("id-do-admin", true)]
    public async Task BuscarDevolveLancamentoParaDonoOuAdmin(string idUsuario, bool eAdmin)
    {
        var conta = _cenario.NovaConta(1);
        var lancamento = _cenario.NovoLancamento(conta, 50m);

        var resultado = await _service.BuscarAsync(lancamento.Id, idUsuario, eAdmin);

        Assert.True(resultado.IsSuccess);
        Assert.Equal(lancamento.Id, resultado.Value.Id);
        Assert.Equal(50m, resultado.Value.Valor);
    }

    [Theory]
    [InlineData(1)]  // existe, mas a conta é de outro usuário
    [InlineData(99)] // não existe
    public async Task BuscarRetornaNaoEncontradoQuandoLancamentoNaoEhAcessivel(int idLancamento)
    {
        var contaDeOutro = _cenario.NovaConta(1, CenarioDeServico.IdOutroUsuario);
        _cenario.NovoLancamento(contaDeOutro, 50m, id: 1);

        var resultado = await _service.BuscarAsync(idLancamento, CenarioDeServico.IdDono, eAdmin: false);

        Assert.True(resultado.HasError<NaoEncontradoError>());
    }

    // ---- Listar ----

    [Fact]
    public async Task ListarDevolveSoOsLancamentosDasContasDoUsuario()
    {
        var minha = _cenario.NovaConta(1);
        var deOutro = _cenario.NovaConta(2, CenarioDeServico.IdOutroUsuario);
        _cenario.NovoLancamento(minha, 10m);
        _cenario.NovoLancamento(minha, -3m);
        _cenario.NovoLancamento(deOutro, 7m);

        var lancamentos = await _service.ListarAsync(CenarioDeServico.IdDono, eAdmin: false);

        Assert.Equal(2, lancamentos.Count);
        Assert.All(lancamentos, l => Assert.Equal(minha.Numero, l.NumeroConta));
    }

    [Fact]
    public async Task ListarDevolveTodosOsLancamentosParaAdmin()
    {
        var minha = _cenario.NovaConta(1);
        var deOutro = _cenario.NovaConta(2, CenarioDeServico.IdOutroUsuario);
        _cenario.NovoLancamento(minha, 10m);
        _cenario.NovoLancamento(deOutro, 7m);

        var lancamentos = await _service.ListarAsync("id-do-admin", eAdmin: true);

        Assert.Equal(2, lancamentos.Count);
    }
}
