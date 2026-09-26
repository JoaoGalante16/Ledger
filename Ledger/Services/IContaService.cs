using Ledger.Data.ContaDtos;

namespace Ledger.Services;

public interface IContaService
{ 
   ReadContaDto Criar(CreateContaDto dto, string idUsuario);
   List<ReadContaDto> Listar(string idUsuario, bool eAdmin);
   bool Atualizar(UpdateContaDto dto, int id);
   bool Remover(int id);
   ReadContaDto?  Buscar(int id);
}