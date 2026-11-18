namespace Ledger.Domain.Repositories;

public record Pagina<T>(List<T> Itens, int TotalItens);
