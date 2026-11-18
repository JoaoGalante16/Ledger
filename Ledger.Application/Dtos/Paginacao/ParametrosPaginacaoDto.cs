using System.ComponentModel.DataAnnotations;

namespace Ledger.Application.Dtos.Paginacao;

public class ParametrosPaginacaoDto
{
    [Range(1, int.MaxValue, ErrorMessage = "A página deve ser maior que zero")]
    public int Pagina { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "O tamanho da página deve ser entre 1 e 100")]
    public int TamanhoPagina { get; set; } = 20;
}
