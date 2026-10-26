using System.Linq.Expressions;
using AutoMapper;
using Bogus;
using Bogus.Extensions.Brazil;
using Ledger.Domain.Constantes;
using Ledger.Domain.Entities;
using Ledger.Domain.Repositories;
using Ledger.Domain.ValueObjects;
using Moq;

namespace Ledger.Tests.Unitarios.Suporte;

public class CenarioDeServico
{
    public const string IdDono = "usuario-dono";
    public const string IdOutroUsuario = "usuario-outro";
    public const int ProximoIdTransacao = 42;
    public const int NumeroContaCaixa = 999;

    private readonly Faker _faker = new("pt_BR");

    public List<Conta> Contas { get; } = new();
    public List<Lancamento> Lancamentos { get; } = new();
    public List<Lancamento> LancamentosAdicionados { get; } = new();

    public Mock<IContaRepository> ContaRepository { get; } = new();
    public Mock<ILancamentoRepository> LancamentoRepository { get; } = new();
    public Mock<IUnitOfWork> UnitOfWork { get; } = new();
    public IMapper Mapper { get; } = MapperDeTeste.Criar();
    public Conta Caixa { get; }

    public CenarioDeServico()
    {
        ContaRepository
            .Setup(r => r.BuscarPorPkAsync(It.IsAny<Expression<Func<Conta, bool>>>()))
            .ReturnsAsync((Expression<Func<Conta, bool>> predicado) => Contas.FirstOrDefault(predicado.Compile()));
        ContaRepository
            .Setup(r => r.BuscarTodosAsync())
            .ReturnsAsync(() => Contas.ToList());
        ContaRepository
            .Setup(r => r.BuscarPorUsuarioAsync(It.IsAny<string>()))
            .ReturnsAsync((string idUsuario) => Contas.Where(c => c.IdUsuario == idUsuario).ToList());

        LancamentoRepository
            .Setup(r => r.BuscarPorIdAsync(It.IsAny<Expression<Func<Lancamento, bool>>>()))
            .ReturnsAsync((Expression<Func<Lancamento, bool>> predicado) => Lancamentos.SingleOrDefault(predicado.Compile()));
        LancamentoRepository
            .Setup(r => r.BuscarTodosAsync())
            .ReturnsAsync(() => Lancamentos.ToList());
        LancamentoRepository
            .Setup(r => r.BuscarPorUsuarioAsync(It.IsAny<string>()))
            .ReturnsAsync((string idUsuario) => Lancamentos.Where(l => l.Conta.IdUsuario == idUsuario).ToList());
        LancamentoRepository
            .Setup(r => r.BuscarPorTransacaoAsync(It.IsAny<int>()))
            .ReturnsAsync((int idTransacao) => Lancamentos.Where(l => l.IdTransacao == idTransacao).OrderBy(l => l.Valor.Value).ToList());
        LancamentoRepository
            .Setup(r => r.SomarSaldoAsync(It.IsAny<int>()))
            .ReturnsAsync((int numeroConta) => Lancamentos.Where(l => l.NumeroConta == numeroConta).Sum(l => l.Valor.Value));
        LancamentoRepository
            .Setup(r => r.SomarSaldosAsync(It.IsAny<List<int>>()))
            .ReturnsAsync((List<int> numerosContas) => Lancamentos
                .Where(l => numerosContas.Contains(l.NumeroConta))
                .GroupBy(l => l.NumeroConta)
                .ToDictionary(g => g.Key, g => g.Sum(l => l.Valor.Value)));
        LancamentoRepository
            .Setup(r => r.ExisteLancamentoNaContaAsync(It.IsAny<int>()))
            .ReturnsAsync((int numeroConta) => Lancamentos.Any(l => l.NumeroConta == numeroConta));
        LancamentoRepository
            .Setup(r => r.AdicionarAsync(It.IsAny<Lancamento>()))
            .Callback<Lancamento>(LancamentosAdicionados.Add)
            .Returns(Task.CompletedTask);
        LancamentoRepository
            .Setup(r => r.ProximoIdTransacaoAsync())
            .ReturnsAsync(ProximoIdTransacao);

        UnitOfWork.Setup(u => u.ContaRepository).Returns(ContaRepository.Object);
        UnitOfWork.Setup(u => u.LancamentoRepository).Returns(LancamentoRepository.Object);

        Caixa = NovaConta(NumeroContaCaixa, ContaCaixa.IdUsuario);
        ContaRepository
            .Setup(r => r.BuscarContaCaixaAsync())
            .ReturnsAsync(() => Caixa);
    }

    public Conta NovaConta(int numero, string idUsuario = IdDono, bool encerrada = false)
    {
        var pessoa = new Person("pt_BR");
        var conta = new Conta
        {
            Numero = numero,
            Nome = pessoa.FullName,
            Cpf = new Cpf(pessoa.Cpf(includeFormatSymbols: false)),
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
            Valor = new Valor(valor),
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
        LancamentoRepository.Verify(r => r.AdicionarAsync(It.IsAny<Lancamento>()), Times.Never);
        UnitOfWork.Verify(u => u.CommitAsync(), Times.Never);
    }
}
