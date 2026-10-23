using AutoMapper;
using FluentResults;
using Ledger.Application.Dtos.Contas;
using Ledger.Application.Errors;
using Ledger.Domain.Entities;
using Ledger.Domain.Repositories;

namespace Ledger.Application.Services;

public class ContaService : IContaService
{
    private readonly IUnitOfWork  _unitOfWork;
    private readonly IMapper _mapper;

    public ContaService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ReadContaDto> Criar(CreateContaDto dto, string idUsuario)
    {
        var conta = _mapper.Map<Conta>(dto);
        conta.IdUsuario = idUsuario;
        await _unitOfWork.ContaRepository.Adicionar(conta);
        await _unitOfWork.Commit();
        return _mapper.Map<ReadContaDto>(conta);

    }

    public async Task<List<ReadContaDto>> Listar(string idUsuario, bool eAdmin)
    {
        List<Conta> contas;
        var query = await _unitOfWork.ContaRepository.BuscarTodos();
        if (eAdmin)
        {
            contas = await _unitOfWork.ContaRepository.BuscarTodos();
        }
        else
        {
            contas = await _unitOfWork.ContaRepository.BuscarPorUsuario(idUsuario);
        }

        var numerosContas = contas.Select(c => c.Numero).ToList();
        var saldos = await _unitOfWork.LancamentoRepository.SomarSaldos(numerosContas);
        
        var readDto = _mapper.Map<List<ReadContaDto>>(contas);
        foreach (var conta in readDto)
        {
            conta.Saldo = saldos.GetValueOrDefault(conta.Numero);
        }
        return readDto;
    }

    public async Task<Result> Atualizar(UpdateContaDto dto, int numeroConta, string idUsuario, bool eAdmin)
    {
        var conta = await BuscarContaAcessivel(numeroConta, idUsuario, eAdmin);
        if (conta is null) return Result.Fail(new NaoEncontradoError("Conta não encontrada"));
        _mapper.Map(dto, conta);
        await _unitOfWork.ContaRepository.Atualizar(conta);
        await _unitOfWork.Commit();
        return Result.Ok();

    }

    public async Task<Result> Remover(int numeroConta, string idUsuario, bool eAdmin)
    {
        var conta = await BuscarContaAcessivel(numeroConta, idUsuario, eAdmin);
        if (conta is null) return Result.Fail(new NaoEncontradoError("Conta não encontrada"));

        var temLancamento = await _unitOfWork.LancamentoRepository.ExisteLancamentoNaConta(numeroConta);
        if (temLancamento)
            return Result.Fail(new ConflitoError("A conta tem lançamentos registrados, use a opção de desativar"));
        
        await _unitOfWork.ContaRepository.Deletar(conta);
        await _unitOfWork.Commit();
        return Result.Ok();
    }

    public async Task<Result<ReadContaDto>> Buscar(int numeroConta, string idUsuario, bool eAdmin)
    {
        var conta = await BuscarContaAcessivel(numeroConta, idUsuario, eAdmin);
        if (conta is null) 
            return Result.Fail(new NaoEncontradoError("Conta não encontrada"));
        var saldo = await _unitOfWork.LancamentoRepository.SomarSaldo(numeroConta);
        var readDto = _mapper.Map<ReadContaDto>(conta);
        readDto.Saldo = saldo;
        return Result.Ok(readDto);
    }

    public async Task<Result> Encerrar(int numeroConta, string idUsuario, bool eAdmin)
    {
        var conta = await BuscarContaAcessivel(numeroConta, idUsuario, eAdmin);
        if (conta is null) 
            return Result.Fail(new NaoEncontradoError("Conta não encontrada"));
        if (conta.DataEncerramento is not null)
            return Result.Fail(new ConflitoError("A conta já está encerrada"));
        var saldo = await _unitOfWork.LancamentoRepository.SomarSaldo(numeroConta);
        if (saldo != 0) return Result.Fail(new ConflitoError("O saldo precisa ser zero para encerrar a conta"));

        conta.DataEncerramento = DateTime.UtcNow;
        await _unitOfWork.Commit();
        return Result.Ok();
    }

    private Task<Conta?> BuscarContaAcessivel(int numeroConta, string idUsuario, bool eAdmin)
    {
        return _unitOfWork.ContaRepository.BuscarPorPk(c =>
            c.Numero == numeroConta && (eAdmin || c.IdUsuario == idUsuario));
    }
    
}