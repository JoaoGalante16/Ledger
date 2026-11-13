using AutoMapper;
using FluentResults;
using Ledger.Application.Dtos.Contas;
using Ledger.Application.Errors;
using Ledger.Domain.Entities;
using Ledger.Domain.Repositories;
using Ledger.Domain.ValueObjects;

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
        var conta = new Conta(dto.Nome, new Cpf(dto.Cpf), idUsuario);
        await _unitOfWork.ContaRepository.AdicionarAsync(conta);
        await _unitOfWork.CommitAsync();
        return _mapper.Map<ReadContaDto>(conta);

    }

    public async Task<List<ReadContaDto>> ListarAsync(string idUsuario, bool eAdmin)
    {
        List<Conta> contas;
        if (eAdmin)
        {
            contas = await _unitOfWork.ContaRepository.BuscarTodosAsync();
        }
        else
        {
            contas = await _unitOfWork.ContaRepository.BuscarPorUsuarioAsync(idUsuario);
        }

        return _mapper.Map<List<ReadContaDto>>(contas);
    }

    public async Task<Result> AtualizarAsync(UpdateContaDto dto, int numeroConta, string idUsuario, bool eAdmin)
    {
        var conta = await BuscarContaAcessivelAsync(numeroConta, idUsuario, eAdmin);
        if (conta is null)
            return Result.Fail(new NaoEncontradoError("Conta não encontrada"));

        conta.Renomear(dto.Nome);
        await _unitOfWork.ContaRepository.AtualizarAsync(conta);
        await _unitOfWork.CommitAsync();
        return Result.Ok();

    }

    public async Task<Result> RemoverAsync(int numeroConta, string idUsuario, bool eAdmin)
    {
        var conta = await BuscarContaAcessivelAsync(numeroConta, idUsuario, eAdmin);
        if (conta is null) return Result.Fail(new NaoEncontradoError("Conta não encontrada"));
        
        var contaCaixa = await _unitOfWork.ContaRepository.BuscarContaCaixaAsync();
        if (conta.Numero == contaCaixa.Numero) 
            return Result.Fail(new ConflitoError("A conta Caixa é do banco e não pode ser alterada"));

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
        return Result.Ok(_mapper.Map<ReadContaDto>(conta));
    }

    public async Task<Result> EncerrarAsync(int numeroConta, string idUsuario, bool eAdmin)
    {
        var conta = await BuscarContaAcessivelAsync(numeroConta, idUsuario, eAdmin);
        if (conta is null)
            return Result.Fail(new NaoEncontradoError("Conta não encontrada"));

        conta.Encerrar();
        await _unitOfWork.CommitAsync();
        return Result.Ok();
    }

    private Task<Conta?> BuscarContaAcessivelAsync(int numeroConta, string idUsuario, bool eAdmin)
    {
        return _unitOfWork.ContaRepository.BuscarPorPkAsync(c =>
            c.Numero == numeroConta && (eAdmin || c.IdUsuario == idUsuario));
    }
    
}