using AutoMapper;
using FluentResults;
using Ledger.Application.Dtos.Contas;
using Ledger.Application.Errors;
using Ledger.Application.Extensions;
using Ledger.Domain.Repositories;

namespace Ledger.Application.UseCases.Contas;

public class BuscarConta
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public BuscarConta(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ReadContaDto>> ExecutarAsync(int numeroConta, string idUsuario, bool eAdmin)
    {
        var conta = await _unitOfWork.ContaRepository.BuscarAcessivelAsync(numeroConta, idUsuario, eAdmin);
        if (conta is null)
            return Result.Fail(new NaoEncontradoError("Conta não encontrada"));
        return Result.Ok(_mapper.Map<ReadContaDto>(conta));
    }
}
