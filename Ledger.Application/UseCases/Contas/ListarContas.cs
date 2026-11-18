using AutoMapper;
using Ledger.Application.Dtos.Contas;
using Ledger.Application.Dtos.Paginacao;
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

    public async Task<PaginaDto<ReadContaDto>> ExecutarAsync(ParametrosPaginacaoDto paginacao, string idUsuario, bool eAdmin)
    {
        var dono = eAdmin ? null : idUsuario;
        var pagina = await _unitOfWork.ContaRepository.BuscarPaginaAsync(paginacao.Pagina, paginacao.TamanhoPagina, dono);

        return new PaginaDto<ReadContaDto>
        {
            Itens = _mapper.Map<List<ReadContaDto>>(pagina.Itens),
            Pagina = paginacao.Pagina,
            TamanhoPagina = paginacao.TamanhoPagina,
            TotalItens = pagina.TotalItens
        };
    }
}
