using System.ComponentModel.DataAnnotations;
using Ledger.Validation;

namespace Ledger.Data.Dtos.LancamentoDtos;

public class CreateLancamentoCorrecaoDto
{
    [Range(0.01 , (double)decimal.MaxValue , ErrorMessage = "O valor deve ser maior que zero")]
    [MaximoDuasCasasDecimais]
    public decimal Valor { get; set; }
    [StringLength(250, ErrorMessage = "A descrição não pode passar de 250 caracteres")]
    public string? Descricao { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "O id do lançamento de referencia deve ser maior que zero")]
    public int IdLancamentoReferencia { get; set; }
    
}