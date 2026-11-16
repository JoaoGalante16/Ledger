using AutoMapper;
using Ledger.Application.Dtos.Contas;
using Ledger.Domain.Entities;
using Ledger.Domain.Repositories;

namespace Ledger.Application.UseCases.Contas;

public class ListarContas
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ListarContas(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<ReadContaDto>> ExecutarAsync(string idUsuario, bool eAdmin)
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
}
