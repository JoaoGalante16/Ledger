using FluentResults;
using Ledger.Application.Dtos.Contas;
using Ledger.Application.Errors;
using Ledger.Application.Extensions;
using Ledger.Domain.Repositories;

namespace Ledger.Application.UseCases.Contas;

public class AtualizarConta
{
    private readonly IUnitOfWork _unitOfWork;

    public AtualizarConta(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> ExecutarAsync(UpdateContaDto dto, int numeroConta, string idUsuario, bool eAdmin)
    {
        var conta = await _unitOfWork.ContaRepository.BuscarAcessivelAsync(numeroConta, idUsuario, eAdmin);
        if (conta is null)
            return Result.Fail(new NaoEncontradoError("Conta não encontrada"));

        conta.Renomear(dto.Nome);
        await _unitOfWork.ContaRepository.AtualizarAsync(conta);
        await _unitOfWork.CommitAsync();
        return Result.Ok();
    }
}
