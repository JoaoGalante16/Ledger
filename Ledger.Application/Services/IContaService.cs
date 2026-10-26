using FluentResults;
using Ledger.Application.Dtos.Contas;

namespace Ledger.Application.Services;

public interface IContaService
{ 
   Task<ReadContaDto> CriarAsync(CreateContaDto dto, string idUsuario);
   Task<List<ReadContaDto>> ListarAsync(string idUsuario, bool eAdmin);
   Task<Result> AtualizarAsync(UpdateContaDto dto, int numeroConta, string idUsuario, bool eAdmin);
   Task<Result> RemoverAsync(int numeroConta, string idUsuario, bool eAdmin);
   Task<Result<ReadContaDto>> BuscarAsync(int numeroConta, string idUsuario, bool eAdmin);
   Task<Result> EncerrarAsync(int numeroConta, string idUsuario, bool eAdmin);
   
}