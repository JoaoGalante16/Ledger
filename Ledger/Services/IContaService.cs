using FluentResults;
using Ledger.Data.Dtos.ContaDtos;

namespace Ledger.Services;

public interface IContaService
{ 
   Task<ReadContaDto> Criar(CreateContaDto dto, string idUsuario);
   Task<List<ReadContaDto>> Listar(string idUsuario, bool eAdmin);
   Task<Result> Atualizar(UpdateContaDto dto, int id, string idUsuario, bool eAdmin);
   Task<Result> Remover(int id, string idUsuario, bool eAdmin);
   Task<Result<ReadContaDto>> Buscar(int id, string idUsuario, bool eAdmin);
   Task<Result> Encerrar(int id, string idUsuario, bool eAdmin);
}