using System.Linq.Expressions;
using AutoMapper;
using Bogus;
using Bogus.Extensions.Brazil;
using Ledger.Data.Repositories.Interfaces;
using Ledger.Data.UnitOfWork;
using Ledger.Models;
using MockQueryable;
using Moq;

namespace Ledger.Tests.Unitarios.Suporte;

public class CenarioDeServico
{
    public const string IdDono = "usuario-dono";
    public const string IdOutroUsuario = "usuario-outro";
    public const int ProximoIdTransacao = 42;

    private readonly Faker _faker = new("pt_BR");

    public List<Conta> Contas { get; } = new();
    public List<Lancamento> Lancamentos { get; } = new();
    public List<Lancamento> LancamentosAdicionados { get; } = new();

    public Mock<IContaRepository> ContaRepository { get; } = new();
    public Mock<ILancamentoRepository> LancamentoRepository { get; } = new();
    public Mock<IUnitOfWork> UnitOfWork { get; } = new();
    public IMapper Mapper { get; } = MapperDeTeste.Criar();

    public CenarioDeServico()
    {
        ContaRepository
            .Setup(r => r.BuscarPorPk(It.IsAny<Expression<Func<Conta, bool>>>()))
            .ReturnsAsync((Expression<Func<Conta, bool>> predicado) => Contas.FirstOrDefault(predicado.Compile()));
        ContaRepository
            .Setup(r => r.BuscarTodos())
            .ReturnsAsync(() => Contas.BuildMock());

        LancamentoRepository
            .Setup(r => r.BuscarPorId(It.IsAny<Expression<Func<Lancamento, bool>>>()))
            .ReturnsAsync((Expression<Func<Lancamento, bool>> predicado) => Lancamentos.SingleOrDefault(predicado.Compile()));
        LancamentoRepository
            .Setup(r => r.BuscarTodos())
            .ReturnsAsync(() => Lancamentos.BuildMock());
        LancamentoRepository
            .Setup(r => r.Adicionar(It.IsAny<Lancamento>()))
            .Callback<Lancamento>(LancamentosAdicionados.Add)
            .Returns(Task.CompletedTask);
        LancamentoRepository
            .Setup(r => r.ProximoIdTransacao())
            .ReturnsAsync(ProximoIdTransacao);

        UnitOfWork.Setup(u => u.ContaRepository).Returns(ContaRepository.Object);
        UnitOfWork.Setup(u => u.LancamentoRepository).Returns(LancamentoRepository.Object);
    }

    public Conta NovaConta(int numero, string idUsuario = IdDono, bool encerrada = false)
    {
        var pessoa = new Person("pt_BR");
        var conta = new Conta
        {
            Numero = numero,
            Nome = pessoa.FullName,
            Cpf = pessoa.Cpf(includeFormatSymbols: false),
            IdUsuario = idUsuario,
            DataEncerramento = encerrada ? DateTime.UtcNow.AddDays(-1) : null
        };
        Contas.Add(conta);
        return conta;
    }

    public Lancamento NovoLancamento(Conta conta, decimal valor, int idTransacao = 1, int? id = null)
    {
        var lancamento = new Lancamento
        {
            Id = id ?? Lancamentos.Count + 1,
            IdTransacao = idTransacao,
            NumeroConta = conta.Numero,
            Conta = conta,
            Valor = valor,
            DataTransacao = DateTime.UtcNow,
            DataGravacao = DateTime.UtcNow,
            Descricao = _faker.Lorem.Sentence()
        };
        Lancamentos.Add(lancamento);
        return lancamento;
    }
    
    public void DarSaldo(Conta conta, decimal saldo) => NovoLancamento(conta, saldo, idTransacao: 900 + Lancamentos.Count);

    public void VerificarQueNadaFoiGravado()
    {
        LancamentoRepository.Verify(r => r.Adicionar(It.IsAny<Lancamento>()), Times.Never);
        UnitOfWork.Verify(u => u.Commit(), Times.Never);
    }
}
