using FluentResults;
using Ledger.Application.Errors;
using Ledger.Application.Extensions;
using Ledger.Domain.Repositories;

namespace Ledger.Application.UseCases.Contas;

public class RemoverConta
{
    private readonly IUnitOfWork _unitOfWork;

    public RemoverConta(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> ExecutarAsync(int numeroConta, string idUsuario, bool eAdmin)
    {
        var conta = await _unitOfWork.ContaRepository.BuscarAcessivelAsync(numeroConta, idUsuario, eAdmin);
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
}
