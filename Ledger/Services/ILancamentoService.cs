using Ledger.Data.LancamentoDtos;

namespace Ledger.Services;

public interface ILancamentoService
{
    List<ReadLancamentoDto>? Criar(CreateLancamentoDto dto);
    List<ReadLancamentoDto> Listar();
    ReadLancamentoDto? Buscar(int id);
    List<ReadLancamentoDto>? BuscarParDeLancamentos(int idTransacao);
    List<ReadLancamentoDto>? CriarLancamentoCorrecao(CreateLancamentoCorrecaoDto dto);
}