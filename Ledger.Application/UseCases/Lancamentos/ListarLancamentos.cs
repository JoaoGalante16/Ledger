using AutoMapper;
using Ledger.Application.Dtos.Lancamentos;
using Ledger.Domain.Entities;
using Ledger.Domain.Repositories;

namespace Ledger.Application.UseCases.Lancamentos;

public class ListarLancamentos
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ListarLancamentos(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<ReadLancamentoDto>> ExecutarAsync(string idUsuario, bool eAdmin)
    {
        List<Lancamento> lancamentos;
        if (eAdmin)
        {
            lancamentos = await _unitOfWork.LancamentoRepository.BuscarTodosAsync();
        }
        else
        {
            lancamentos = await _unitOfWork.LancamentoRepository.BuscarPorUsuarioAsync(idUsuario);
        }

        return _mapper.Map<List<ReadLancamentoDto>>(lancamentos);
    }
}
