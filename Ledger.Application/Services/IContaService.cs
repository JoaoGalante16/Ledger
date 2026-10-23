using FluentResults;
using Ledger.Application.Dtos.Contas;

namespace Ledger.Application.Services;

public interface IContaService
{ 
   Task<ReadContaDto> Criar(CreateContaDto dto, string idUsuario);
   Task<List<ReadContaDto>> Listar(string idUsuario, bool eAdmin);
   Task<Result> Atualizar(UpdateContaDto dto, int numeroConta, string idUsuario, bool eAdmin);
   Task<Result> Remover(int numeroConta, string idUsuario, bool eAdmin);
   Task<Result<ReadContaDto>> Buscar(int numeroConta, string idUsuario, bool eAdmin);
   Task<Result> Encerrar(int numeroConta, string idUsuario, bool eAdmin);
   
}