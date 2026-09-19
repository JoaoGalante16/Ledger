using Ledger.Data.ContaDtos;

namespace Ledger.Services;

public interface IContaService
{ 
   ReadContaDto Criar(CreateContaDto dto);
   List<ReadContaDto> Listar();
   bool Atualizar(UpdateContaDto dto, int id);
   bool Remover(int id);
   ReadContaDto?  Buscar(int id);
}