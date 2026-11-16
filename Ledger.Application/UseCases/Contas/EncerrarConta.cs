using FluentResults;
using Ledger.Application.Errors;
using Ledger.Application.Extensions;
using Ledger.Domain.Repositories;

namespace Ledger.Application.UseCases.Contas;

public class EncerrarConta
{
    private readonly IUnitOfWork _unitOfWork;

    public EncerrarConta(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> ExecutarAsync(int numeroConta, string idUsuario, bool eAdmin)
    {
        var conta = await _unitOfWork.ContaRepository.BuscarAcessivelAsync(numeroConta, idUsuario, eAdmin);
        if (conta is null)
            return Result.Fail(new NaoEncontradoError("Conta não encontrada"));

        conta.Encerrar();
        await _unitOfWork.CommitAsync();
        return Result.Ok();
    }
}
