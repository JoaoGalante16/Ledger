namespace Ledger.Application.Dtos.Paginacao;

public class PaginaDto<T>
{
    public List<T> Itens { get; init; } = new();
    public int Pagina { get; init; }
    public int TamanhoPagina { get; init; }
    public int TotalItens { get; init; }
    public int TotalPaginas => (int)Math.Ceiling(TotalItens / (double)TamanhoPagina);
}
