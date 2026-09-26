using Ledger.Data.LancamentoDtos;

namespace Ledger.Services;

public interface ILancamentoService
{
    List<ReadLancamentoDto>? Criar(CreateLancamentoDto dto, string? idUsuario, bool eAdmin);
    List<ReadLancamentoDto> Listar(string? idUsuario, bool eAdmin);
    ReadLancamentoDto? Buscar(int id, string? idUsuario, bool eAdmin);
    List<ReadLancamentoDto>? BuscarParDeLancamentos(int idTransacao);
    List<ReadLancamentoDto>? CriarLancamentoCorrecao(CreateLancamentoCorrecaoDto dto);
}