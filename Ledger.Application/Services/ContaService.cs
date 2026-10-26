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

    public async Task<ReadContaDto> CriarAsync(CreateContaDto dto, string idUsuario)
    {
        var conta = _mapper.Map<Conta>(dto);
        conta.IdUsuario = idUsuario;
        await _unitOfWork.ContaRepository.AdicionarAsync(conta);
        await _unitOfWork.CommitAsync();
        return _mapper.Map<ReadContaDto>(conta);

    }

    public async Task<List<ReadContaDto>> ListarAsync(string idUsuario, bool eAdmin)
    {
        List<Conta> contas;
        var query = await _unitOfWork.ContaRepository.BuscarTodosAsync();
        if (eAdmin)
        {
            contas = await _unitOfWork.ContaRepository.BuscarTodosAsync();
        }
        else
        {
            contas = await _unitOfWork.ContaRepository.BuscarPorUsuarioAsync(idUsuario);
        }

        var numerosContas = contas.Select(c => c.Numero).ToList();
        var saldos = await _unitOfWork.LancamentoRepository.SomarSaldosAsync(numerosContas);
        
        var readDto = _mapper.Map<List<ReadContaDto>>(contas);
        foreach (var conta in readDto)
        {
            conta.Saldo = saldos.GetValueOrDefault(conta.Numero);
        }
        return readDto;
    }

    public async Task<Result> AtualizarAsync(UpdateContaDto dto, int numeroConta, string idUsuario, bool eAdmin)
    {
        var conta = await BuscarContaAcessivelAsync(numeroConta, idUsuario, eAdmin);
        if (conta is null) return Result.Fail(new NaoEncontradoError("Conta não encontrada"));
        _mapper.Map(dto, conta);
        await _unitOfWork.ContaRepository.AtualizarAsync(conta);
        await _unitOfWork.CommitAsync();
        return Result.Ok();

    }

    public async Task<Result> RemoverAsync(int numeroConta, string idUsuario, bool eAdmin)
    {
        var conta = await BuscarContaAcessivelAsync(numeroConta, idUsuario, eAdmin);
        if (conta is null) return Result.Fail(new NaoEncontradoError("Conta não encontrada"));

        var temLancamento = await _unitOfWork.LancamentoRepository.ExisteLancamentoNaContaAsync(numeroConta);
        if (temLancamento)
            return Result.Fail(new ConflitoError("A conta tem lançamentos registrados, use a opção de desativar"));
        
        await _unitOfWork.ContaRepository.DeletarAsync(conta);
        await _unitOfWork.CommitAsync();
        return Result.Ok();
    }

    public async Task<Result<ReadContaDto>> BuscarAsync(int numeroConta, string idUsuario, bool eAdmin)
    {
        var conta = await BuscarContaAcessivelAsync(numeroConta, idUsuario, eAdmin);
        if (conta is null) 
            return Result.Fail(new NaoEncontradoError("Conta não encontrada"));
        var saldo = await _unitOfWork.LancamentoRepository.SomarSaldoAsync(numeroConta);
        var readDto = _mapper.Map<ReadContaDto>(conta);
        readDto.Saldo = saldo;
        return Result.Ok(readDto);
    }

    public async Task<Result> EncerrarAsync(int numeroConta, string idUsuario, bool eAdmin)
    {
        var conta = await BuscarContaAcessivelAsync(numeroConta, idUsuario, eAdmin);
        if (conta is null) 
            return Result.Fail(new NaoEncontradoError("Conta não encontrada"));
        if (conta.DataEncerramento is not null)
            return Result.Fail(new ConflitoError("A conta já está encerrada"));
        var saldo = await _unitOfWork.LancamentoRepository.SomarSaldoAsync(numeroConta);
        if (saldo != 0) return Result.Fail(new ConflitoError("O saldo precisa ser zero para encerrar a conta"));

        conta.DataEncerramento = DateTime.UtcNow;
        await _unitOfWork.CommitAsync();
        return Result.Ok();
    }

    private Task<Conta?> BuscarContaAcessivelAsync(int numeroConta, string idUsuario, bool eAdmin)
    {
        return _unitOfWork.ContaRepository.BuscarPorPkAsync(c =>
            c.Numero == numeroConta && (eAdmin || c.IdUsuario == idUsuario));
    }
    
}