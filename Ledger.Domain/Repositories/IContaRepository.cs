using Ledger.Domain.Entities;

namespace Ledger.Domain.Repositories;

public interface IContaRepository : IRepository<Conta>
{
    Task<List<Conta>> BuscarPorUsuario(string idUsuario);
    
}