using FluentResults;
using Ledger.Data.ContaDtos;

namespace Ledger.Services;

public interface IContaService
{ 
   ReadContaDto Criar(CreateContaDto dto, string idUsuario);
   List<ReadContaDto> Listar(string idUsuario, bool eAdmin);
   Result Atualizar(UpdateContaDto dto, int id, string idUsuario, bool eAdmin);
   Result Remover(int id, string idUsuario, bool eAdmin);
   Result<ReadContaDto>  Buscar(int id, string idUsuario, bool eAdmin);
}