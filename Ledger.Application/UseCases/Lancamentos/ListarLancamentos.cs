using AutoMapper;
using Ledger.Application.Dtos.Lancamentos;
using Ledger.Application.Dtos.Paginacao;
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

    public async Task<PaginaDto<ReadLancamentoDto>> ExecutarAsync(ParametrosPaginacaoDto paginacao, string idUsuario, bool eAdmin)
    {
        var dono = eAdmin ? null : idUsuario;
        var pagina = await _unitOfWork.LancamentoRepository.BuscarPaginaAsync(paginacao.Pagina, paginacao.TamanhoPagina, dono);

        return new PaginaDto<ReadLancamentoDto>
        {
            Itens = _mapper.Map<List<ReadLancamentoDto>>(pagina.Itens),
            Pagina = paginacao.Pagina,
            TamanhoPagina = paginacao.TamanhoPagina,
            TotalItens = pagina.TotalItens
        };
    }
}
