using Ledger.Domain.Entities;

namespace Ledger.Domain.Repositories;

public interface IContaRepository : IRepository<Conta>
{
    Task<Pagina<Conta>> BuscarPaginaAsync(int pagina, int tamanhoPagina, string? idUsuario = null);
    Task<Conta> BuscarContaCaixaAsync();
    Task<int> BuscarNumeroContaCaixaAsync();
    Task<List<Conta>> BloquearEBuscarAsync(params int[] numeros);
}