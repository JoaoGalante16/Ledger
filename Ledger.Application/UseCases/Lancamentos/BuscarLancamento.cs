using AutoMapper;
using FluentResults;
using Ledger.Application.Dtos.Lancamentos;
using Ledger.Application.Errors;
using Ledger.Domain.Repositories;

namespace Ledger.Application.UseCases.Lancamentos;

public class BuscarLancamento
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public BuscarLancamento(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ReadLancamentoDto>> ExecutarAsync(int id, string idUsuario, bool eAdmin)
    {
        var lancamento = await _unitOfWork.LancamentoRepository.BuscarPorIdAsync(l =>
            l.Id == id && (eAdmin || l.Conta.IdUsuario == idUsuario));
        if (lancamento is null) return Result.Fail(new NaoEncontradoError("Lancamento não encontrado"));
        return Result.Ok(_mapper.Map<ReadLancamentoDto>(lancamento));
    }
}
