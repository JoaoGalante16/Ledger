using AutoMapper;
using FluentResults;
using Ledger.Application.Dtos.Lancamentos;
using Ledger.Application.Errors;
using Ledger.Domain.Repositories;

namespace Ledger.Application.UseCases.Lancamentos;

public class BuscarParDaTransacao
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public BuscarParDaTransacao(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<List<ReadLancamentoDto>>> ExecutarAsync(int idTransacao)
    {
        var parLancamento = await _unitOfWork.LancamentoRepository.BuscarPorTransacaoAsync(idTransacao);
        if (parLancamento.Count != 2) return Result.Fail(new NaoEncontradoError("Par de lancamento não encontrado"));
        return Result.Ok(_mapper.Map<List<ReadLancamentoDto>>(parLancamento));
    }
}
