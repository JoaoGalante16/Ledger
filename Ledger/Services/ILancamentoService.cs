using FluentResults;
using Ledger.Data.Dtos.LancamentoDtos;

namespace Ledger.Services;

public interface ILancamentoService
{
    Task<Result<List<ReadLancamentoDto>>> Criar(CreateLancamentoDto dto, string idUsuario, bool eAdmin);
    Task<List<ReadLancamentoDto>> Listar(string idUsuario, bool eAdmin);
    Task<Result<ReadLancamentoDto>> Buscar(int id, string idUsuario, bool eAdmin);
    Task<Result<List<ReadLancamentoDto>>> BuscarParDeLancamentos(int idTransacao);
    Task<Result<List<ReadLancamentoDto>>> CriarLancamentoCorrecao(CreateLancamentoCorrecaoDto dto);
}