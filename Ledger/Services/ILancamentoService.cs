using FluentResults;
using Ledger.Data.LancamentoDtos;

namespace Ledger.Services;

public interface ILancamentoService
{
    Result<List<ReadLancamentoDto>> Criar(CreateLancamentoDto dto, string idUsuario, bool eAdmin);
    List<ReadLancamentoDto> Listar(string idUsuario, bool eAdmin);
    Result<ReadLancamentoDto> Buscar(int id, string idUsuario, bool eAdmin);
    Result<List<ReadLancamentoDto>> BuscarParDeLancamentos(int idTransacao);
    Result<List<ReadLancamentoDto>> CriarLancamentoCorrecao(CreateLancamentoCorrecaoDto dto);
}