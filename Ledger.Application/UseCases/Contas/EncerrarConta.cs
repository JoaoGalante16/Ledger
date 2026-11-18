using FluentResults;
using Ledger.Application.Errors;
using Ledger.Application.Extensions;
using Ledger.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Ledger.Application.UseCases.Contas;

public class EncerrarConta
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EncerrarConta> _logger;

    public EncerrarConta(IUnitOfWork unitOfWork, ILogger<EncerrarConta> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> ExecutarAsync(int numeroConta, string idUsuario, bool eAdmin)
    {
        await _unitOfWork.IniciarTransacaoAsync();
        var contas = await _unitOfWork.ContaRepository.BloquearEBuscarAsync(numeroConta);

        var conta = contas.SingleOrDefault(c => c.PodeSerAcessadaPor(idUsuario, eAdmin));
        if (conta is null)
            return Result.Fail(new NaoEncontradoError("Conta não encontrada"));

        conta.Encerrar();
        await _unitOfWork.CommitAsync();
        _logger.LogInformation("Conta {NumeroConta} encerrada", conta.Numero);
        return Result.Ok();
    }
}
