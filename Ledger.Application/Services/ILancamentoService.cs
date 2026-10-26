using FluentResults;
using Ledger.Application.Dtos.Lancamentos;

namespace Ledger.Application.Services;

public interface ILancamentoService
{
    Task<Result<List<ReadLancamentoDto>>> CriarAsync(CreateLancamentoDto dto, string idUsuario, bool eAdmin);
    Task<List<ReadLancamentoDto>> ListarAsync(string idUsuario, bool eAdmin);
    Task<Result<ReadLancamentoDto>> BuscarAsync(int id, string idUsuario, bool eAdmin);
    Task<Result<List<ReadLancamentoDto>>> BuscarParDeLancamentosAsync(int idTransacao);
    Task<Result<List<ReadLancamentoDto>>> CriarLancamentoCorrecaoAsync(CreateLancamentoCorrecaoDto dto);

    Task<Result<List<ReadLancamentoDto>>> DepositarAsync(CreateLancamentoDepositoOuSaque dto, string idUsuario,
        bool eAdmin);
    Task<Result<List<ReadLancamentoDto>>> SacarAsync(CreateLancamentoDepositoOuSaque dto, string idUsuario,
        bool eAdmin);
}