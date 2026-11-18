using Ledger.Application.Dtos.Contas;
using Ledger.Application.Dtos.Paginacao;
using Ledger.Application.Errors;
using Ledger.Application.UseCases.Contas;
using Ledger.Domain.Entities;
using Ledger.Domain.Exceptions;
using Ledger.Tests.Unitarios.Suporte;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Ledger.Tests.Unitarios.UseCases.Contas;

public class CasosDeUsoDeContaTestes
{
    private readonly CenarioDeServico _cenario = new();
    private readonly CriarConta _criarConta;
    private readonly ListarContas _listarContas;
    private readonly AtualizarConta _atualizarConta;
    private readonly RemoverConta _removerConta;
    private readonly BuscarConta _buscarConta;
    private readonly EncerrarConta _encerrarConta;

    public CasosDeUsoDeContaTestes()
    {
        _criarConta = new CriarConta(_cenario.UnitOfWork.Object, _cenario.Mapper, NullLogger<CriarConta>.Instance);
        _listarContas = new ListarContas(_cenario.UnitOfWork.Object, _cenario.Mapper);
        _atualizarConta = new AtualizarConta(_cenario.UnitOfWork.Object);
        _removerConta = new RemoverConta(_cenario.UnitOfWork.Object);
        _buscarConta = new BuscarConta(_cenario.UnitOfWork.Object, _cenario.Mapper);
        _encerrarConta = new EncerrarConta(_cenario.UnitOfWork.Object, NullLogger<EncerrarConta>.Instance);
    }

    // ---- Criar ----

    [Fact]
    public async Task CriarUsaOIdDoUsuarioLogadoComoDono()
    {
        var dto = new CreateContaDto { Nome = "Conta corrente", Cpf = "52998224725" };
        Conta? contaAdicionada = null;
        _cenario.ContaRepository
            .Setup(r => r.AdicionarAsync(It.IsAny<Conta>()))
            .Callback<Conta>(c => contaAdicionada = c)
            .Returns(Task.CompletedTask);

        var resultado = await _criarConta.ExecutarAsync(dto, CenarioDeServico.IdDono);

        Assert.NotNull(contaAdicionada);
        Assert.Equal(CenarioDeServico.IdDono, contaAdicionada.IdUsuario);
        Assert.Equal("Conta corrente", resultado.Nome);
        Assert.Equal("52998224725", resultado.Cpf);
        Assert.Equal(0m, resultado.Saldo);
        _cenario.UnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task CriarNaoLogaOCpfNemONome()
    {
        var logger = new LoggerDeTeste<CriarConta>();
        var criarConta = new CriarConta(_cenario.UnitOfWork.Object, _cenario.Mapper, logger);

        await criarConta.ExecutarAsync(new CreateContaDto { Nome = "Maria Souza", Cpf = "52998224725" }, CenarioDeServico.IdDono);

        var registro = Assert.Single(logger.Registros);
        Assert.DoesNotContain("52998224725", registro.Mensagem);
        Assert.DoesNotContain("Maria Souza", registro.Mensagem);
        Assert.DoesNotContain(registro.Campos.Values, valor => valor is "52998224725" or "Maria Souza");
    }

    // ---- Listar ----

    [Fact]
    public async Task ListarSomaOSaldoDeCadaContaDoUsuario()
    {
        var corrente = _cenario.NovaConta(1);
        var poupanca = _cenario.NovaConta(2);
        _cenario.NovaConta(3);
        var contaDeOutroUsuario = _cenario.NovaConta(4, CenarioDeServico.IdOutroUsuario);
        _cenario.NovoLancamento(corrente, 100m);
        _cenario.NovoLancamento(corrente, -30.25m);
        _cenario.NovoLancamento(poupanca, 50m);
        _cenario.NovoLancamento(contaDeOutroUsuario, 1000m);

        var contas = (await _listarContas.ExecutarAsync(new ParametrosPaginacaoDto(), CenarioDeServico.IdDono, eAdmin: false)).Itens;

        Assert.Equal(new[] { 1, 2, 3 }, contas.Select(c => c.Numero));
        Assert.Equal(69.75m, contas.Single(c => c.Numero == 1).Saldo);
        Assert.Equal(50m, contas.Single(c => c.Numero == 2).Saldo);
        Assert.Equal(0m, contas.Single(c => c.Numero == 3).Saldo);
    }

    [Fact]
    public async Task ListarDevolveTodasAsContasParaAdmin()
    {
        _cenario.NovaConta(1);
        var deOutro = _cenario.NovaConta(2, CenarioDeServico.IdOutroUsuario);
        _cenario.NovoLancamento(deOutro, 40m);

        var contas = (await _listarContas.ExecutarAsync(new ParametrosPaginacaoDto(), "id-do-admin", eAdmin: true)).Itens;

        Assert.Equal(3, contas.Count);
        Assert.Contains(contas, c => c.Numero == CenarioDeServico.NumeroContaCaixa);
        Assert.Equal(40m, contas.Single(c => c.Numero == 2).Saldo);
    }

    [Fact]
    public async Task ListarDevolveAPaginaPedidaComOTotal()
    {
        for (var numero = 1; numero <= 5; numero++)
            _cenario.NovaConta(numero);

        var pagina = await _listarContas.ExecutarAsync(new ParametrosPaginacaoDto { Pagina = 3, TamanhoPagina = 2 }, CenarioDeServico.IdDono, eAdmin: false);

        Assert.Equal(new[] { 5 }, pagina.Itens.Select(c => c.Numero));
        Assert.Equal(3, pagina.Pagina);
        Assert.Equal(2, pagina.TamanhoPagina);
        Assert.Equal(5, pagina.TotalItens);
        Assert.Equal(3, pagina.TotalPaginas);
    }

    // ---- Atualizar ----

    [Fact]
    public async Task AtualizarMudaSoONome()
    {
        var conta = _cenario.NovaConta(1);
        var cpfOriginal = conta.Cpf;

        var resultado = await _atualizarConta.ExecutarAsync(new UpdateContaDto { Nome = "Novo nome" }, 1, CenarioDeServico.IdDono, eAdmin: false);

        Assert.True(resultado.IsSuccess);
        Assert.Equal("Novo nome", conta.Nome);
        Assert.Equal(cpfOriginal, conta.Cpf);
        Assert.Equal(CenarioDeServico.IdDono, conta.IdUsuario);
        Assert.Null(conta.DataEncerramento);
        _cenario.ContaRepository.Verify(r => r.AtualizarAsync(conta), Times.Once);
        _cenario.UnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Theory]
    [InlineData(99)]
    [InlineData(2)] 
    public async Task AtualizarRetornaNaoEncontradoQuandoContaNaoEhAcessivel(int numero)
    {
        _cenario.NovaConta(2, CenarioDeServico.IdOutroUsuario);

        var resultado = await _atualizarConta.ExecutarAsync(new UpdateContaDto { Nome = "X" }, numero, CenarioDeServico.IdDono, eAdmin: false);

        Assert.True(resultado.HasError<NaoEncontradoError>());
        _cenario.ContaRepository.Verify(r => r.AtualizarAsync(It.IsAny<Conta>()), Times.Never);
        _cenario.UnitOfWork.Verify(u => u.CommitAsync(), Times.Never);
    }

    // ---- Remover ----

    [Fact]
    public async Task RemoverApagaContaSemLancamentos()
    {
        var conta = _cenario.NovaConta(1);

        var resultado = await _removerConta.ExecutarAsync(1, CenarioDeServico.IdDono, eAdmin: false);

        Assert.True(resultado.IsSuccess);
        _cenario.ContaRepository.Verify(r => r.DeletarAsync(conta), Times.Once);
        _cenario.UnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoverRetornaConflitoQuandoContaTemLancamentos()
    {
        var conta = _cenario.NovaConta(1);
        _cenario.NovoLancamento(conta, 10m);
        _cenario.NovoLancamento(conta, -10m); 

        var resultado = await _removerConta.ExecutarAsync(1, CenarioDeServico.IdDono, eAdmin: false);

        Assert.True(resultado.HasError<ConflitoError>());
        _cenario.ContaRepository.Verify(r => r.DeletarAsync(It.IsAny<Conta>()), Times.Never);
        _cenario.UnitOfWork.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task RemoverRetornaNaoEncontradoQuandoContaNaoExiste()
    {
        var resultado = await _removerConta.ExecutarAsync(99, CenarioDeServico.IdDono, eAdmin: false);

        Assert.True(resultado.HasError<NaoEncontradoError>());
        _cenario.ContaRepository.Verify(r => r.DeletarAsync(It.IsAny<Conta>()), Times.Never);
    }

    // ---- Buscar ----

    [Fact]
    public async Task BuscarDevolveContaComSaldoCalculado()
    {
        var conta = _cenario.NovaConta(1);
        var outra = _cenario.NovaConta(2);
        _cenario.NovoLancamento(conta, 200m);
        _cenario.NovoLancamento(conta, -50.50m);
        _cenario.NovoLancamento(outra, 1000m);

        var resultado = await _buscarConta.ExecutarAsync(1, CenarioDeServico.IdDono, eAdmin: false);

        Assert.True(resultado.IsSuccess);
        Assert.Equal(149.50m, resultado.Value.Saldo);
        Assert.Equal(conta.Nome, resultado.Value.Nome);
    }

    [Fact]
    public async Task BuscarRetornaNaoEncontradoQuandoContaEhDeOutroUsuario()
    {
        _cenario.NovaConta(1, CenarioDeServico.IdOutroUsuario);

        var resultado = await _buscarConta.ExecutarAsync(1, CenarioDeServico.IdDono, eAdmin: false);

        Assert.True(resultado.HasError<NaoEncontradoError>());
    }

    // ---- Encerrar ----

    [Fact]
    public async Task EncerrarPreencheDataQuandoSaldoEhZero()
    {
        var conta = _cenario.NovaConta(1);
        _cenario.NovoLancamento(conta, 100m);
        _cenario.NovoLancamento(conta, -100m);
        var antes = DateTime.UtcNow;

        var resultado = await _encerrarConta.ExecutarAsync(1, CenarioDeServico.IdDono, eAdmin: false);

        Assert.True(resultado.IsSuccess);
        Assert.NotNull(conta.DataEncerramento);
        Assert.InRange(conta.DataEncerramento.Value, antes, DateTime.UtcNow);
        _cenario.UnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Theory]
    [InlineData("0.01")]
    [InlineData("-0.01")]
    public async Task EncerrarLancaExcecaoQuandoSaldoDiferenteDeZero(string saldo)
    {
        var conta = _cenario.NovaConta(1);
        _cenario.NovoLancamento(conta, decimal.Parse(saldo, System.Globalization.CultureInfo.InvariantCulture));

        await Assert.ThrowsAsync<SaldoEmContaExistenteException>(() =>
            _encerrarConta.ExecutarAsync(1, CenarioDeServico.IdDono, eAdmin: false));

        Assert.Null(conta.DataEncerramento);
        _cenario.UnitOfWork.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task EncerrarLancaExcecaoQuandoContaJaEstaEncerrada()
    {
        var conta = _cenario.NovaConta(1, encerrada: true);
        var dataOriginal = conta.DataEncerramento;

        await Assert.ThrowsAsync<ContaEncerradaException>(() =>
            _encerrarConta.ExecutarAsync(1, CenarioDeServico.IdDono, eAdmin: false));

        Assert.Equal(dataOriginal, conta.DataEncerramento);
        _cenario.UnitOfWork.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task EncerrarRetornaNaoEncontradoQuandoContaNaoExiste()
    {
        var resultado = await _encerrarConta.ExecutarAsync(99, CenarioDeServico.IdDono, eAdmin: false);

        Assert.True(resultado.HasError<NaoEncontradoError>());
        _cenario.UnitOfWork.Verify(u => u.CommitAsync(), Times.Never);
    }

    // ---- Conta Caixa ----

    [Theory]
    [InlineData("atualizar")]
    [InlineData("encerrar")]
    public async Task AdminNaoAlteraAContaCaixa(string operacao)
    {
        var numero = CenarioDeServico.NumeroContaCaixa;

        await Assert.ThrowsAsync<OperacaoNaoPermitidaException>(() => operacao == "atualizar"
            ? _atualizarConta.ExecutarAsync(new UpdateContaDto { Nome = "Outro nome" }, numero, "id-do-admin", eAdmin: true)
            : _encerrarConta.ExecutarAsync(numero, "id-do-admin", eAdmin: true));

        Assert.Null(_cenario.Caixa.DataEncerramento);
        _cenario.UnitOfWork.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task AdminNaoRemoveAContaCaixa()
    {
        var resultado = await _removerConta.ExecutarAsync(CenarioDeServico.NumeroContaCaixa, "id-do-admin", eAdmin: true);

        Assert.True(resultado.HasError<ConflitoError>());
        Assert.Equal("A conta Caixa é do banco e não pode ser alterada", resultado.Errors[0].Message);
        _cenario.ContaRepository.Verify(r => r.DeletarAsync(It.IsAny<Conta>()), Times.Never);
        _cenario.UnitOfWork.Verify(u => u.CommitAsync(), Times.Never);
    }
}
