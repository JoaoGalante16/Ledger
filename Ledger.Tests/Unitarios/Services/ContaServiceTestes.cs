using Ledger.Application.Dtos.Contas;
using Ledger.Application.Errors;
using Ledger.Application.Services;
using Ledger.Domain.Entities;
using Ledger.Tests.Unitarios.Suporte;
using Moq;

namespace Ledger.Tests.Unitarios.Services;

public class ContaServiceTestes
{
    private readonly CenarioDeServico _cenario = new();
    private readonly ContaService _service;

    public ContaServiceTestes()
    {
        _service = new ContaService(_cenario.UnitOfWork.Object, _cenario.Mapper);
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

        var resultado = await _service.CriarAsync(dto, CenarioDeServico.IdDono);

        Assert.NotNull(contaAdicionada);
        Assert.Equal(CenarioDeServico.IdDono, contaAdicionada.IdUsuario);
        Assert.Equal("Conta corrente", resultado.Nome);
        Assert.Equal("52998224725", resultado.Cpf);
        Assert.Equal(0m, resultado.Saldo);
        _cenario.UnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
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

        var contas = await _service.ListarAsync(CenarioDeServico.IdDono, eAdmin: false);

        Assert.Equal(new[] { 1, 2, 3 }, contas.Select(c => c.Numero).Order());
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

        var contas = await _service.ListarAsync("id-do-admin", eAdmin: true);

        Assert.Equal(3, contas.Count);
        Assert.Contains(contas, c => c.Numero == CenarioDeServico.NumeroContaCaixa);
        Assert.Equal(40m, contas.Single(c => c.Numero == 2).Saldo);
    }

    // ---- Atualizar ----

    [Fact]
    public async Task AtualizarMudaSoONome()
    {
        var conta = _cenario.NovaConta(1);
        var cpfOriginal = conta.Cpf;

        var resultado = await _service.AtualizarAsync(new UpdateContaDto { Nome = "Novo nome" }, 1, CenarioDeServico.IdDono, eAdmin: false);

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

        var resultado = await _service.AtualizarAsync(new UpdateContaDto { Nome = "X" }, numero, CenarioDeServico.IdDono, eAdmin: false);

        Assert.True(resultado.HasError<NaoEncontradoError>());
        _cenario.ContaRepository.Verify(r => r.AtualizarAsync(It.IsAny<Conta>()), Times.Never);
        _cenario.UnitOfWork.Verify(u => u.CommitAsync(), Times.Never);
    }

    // ---- Remover ----

    [Fact]
    public async Task RemoverApagaContaSemLancamentos()
    {
        var conta = _cenario.NovaConta(1);

        var resultado = await _service.RemoverAsync(1, CenarioDeServico.IdDono, eAdmin: false);

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

        var resultado = await _service.RemoverAsync(1, CenarioDeServico.IdDono, eAdmin: false);

        Assert.True(resultado.HasError<ConflitoError>());
        _cenario.ContaRepository.Verify(r => r.DeletarAsync(It.IsAny<Conta>()), Times.Never);
        _cenario.UnitOfWork.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task RemoverRetornaNaoEncontradoQuandoContaNaoExiste()
    {
        var resultado = await _service.RemoverAsync(99, CenarioDeServico.IdDono, eAdmin: false);

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

        var resultado = await _service.BuscarAsync(1, CenarioDeServico.IdDono, eAdmin: false);

        Assert.True(resultado.IsSuccess);
        Assert.Equal(149.50m, resultado.Value.Saldo);
        Assert.Equal(conta.Nome, resultado.Value.Nome);
    }

    [Fact]
    public async Task BuscarRetornaNaoEncontradoQuandoContaEhDeOutroUsuario()
    {
        _cenario.NovaConta(1, CenarioDeServico.IdOutroUsuario);

        var resultado = await _service.BuscarAsync(1, CenarioDeServico.IdDono, eAdmin: false);

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

        var resultado = await _service.EncerrarAsync(1, CenarioDeServico.IdDono, eAdmin: false);

        Assert.True(resultado.IsSuccess);
        Assert.NotNull(conta.DataEncerramento);
        Assert.InRange(conta.DataEncerramento.Value, antes, DateTime.UtcNow);
        _cenario.UnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Theory]
    [InlineData("0.01")]
    [InlineData("-0.01")]
    public async Task EncerrarRetornaConflitoQuandoSaldoDiferenteDeZero(string saldo)
    {
        var conta = _cenario.NovaConta(1);
        _cenario.NovoLancamento(conta, decimal.Parse(saldo, System.Globalization.CultureInfo.InvariantCulture));

        var resultado = await _service.EncerrarAsync(1, CenarioDeServico.IdDono, eAdmin: false);

        Assert.True(resultado.HasError<ConflitoError>());
        Assert.Null(conta.DataEncerramento);
        _cenario.UnitOfWork.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task EncerrarRetornaConflitoQuandoContaJaEstaEncerrada()
    {
        var conta = _cenario.NovaConta(1, encerrada: true);
        var dataOriginal = conta.DataEncerramento;

        var resultado = await _service.EncerrarAsync(1, CenarioDeServico.IdDono, eAdmin: false);

        Assert.True(resultado.HasError<ConflitoError>());
        Assert.Equal(dataOriginal, conta.DataEncerramento);
        _cenario.UnitOfWork.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task EncerrarRetornaNaoEncontradoQuandoContaNaoExiste()
    {
        var resultado = await _service.EncerrarAsync(99, CenarioDeServico.IdDono, eAdmin: false);

        Assert.True(resultado.HasError<NaoEncontradoError>());
        _cenario.UnitOfWork.Verify(u => u.CommitAsync(), Times.Never);
    }

    // ---- Conta Caixa ----

    [Theory]
    [InlineData("atualizar")]
    [InlineData("remover")]
    [InlineData("encerrar")]
    public async Task AdminNaoAlteraAContaCaixa(string operacao)
    {
        var numero = CenarioDeServico.NumeroContaCaixa;

        var resultado = operacao switch
        {
            "atualizar" => await _service.AtualizarAsync(new UpdateContaDto { Nome = "Outro nome" }, numero, "id-do-admin", eAdmin: true),
            "remover" => await _service.RemoverAsync(numero, "id-do-admin", eAdmin: true),
            _ => await _service.EncerrarAsync(numero, "id-do-admin", eAdmin: true)
        };

        Assert.True(resultado.HasError<ConflitoError>());
        Assert.Equal("A conta Caixa é do banco e não pode ser alterada", resultado.Errors[0].Message);
        Assert.Null(_cenario.Caixa.DataEncerramento);
        _cenario.ContaRepository.Verify(r => r.AtualizarAsync(It.IsAny<Conta>()), Times.Never);
        _cenario.ContaRepository.Verify(r => r.DeletarAsync(It.IsAny<Conta>()), Times.Never);
        _cenario.UnitOfWork.Verify(u => u.CommitAsync(), Times.Never);
    }
}
