using System.ComponentModel.DataAnnotations;

namespace Ledger.Data.LancamentoDtos;

public class CreateLancamentoCorrecaoDto
{

    [Required(ErrorMessage = "O valor é obrigatório")]
    [Range(0.01 , (double)decimal.MaxValue , ErrorMessage = "O valor deve ser maior que zero")]
    public decimal Valor { get; set; }
    public DateTime DataTransacao { get; set; } = DateTime.UtcNow;
    [StringLength(250, ErrorMessage = "A descrição não pode passar de 250 caracteres")]
    public string? Descricao { get; set; }
    [Required(ErrorMessage = "o id do lançamento de referência é obrigatório")]
    [Range(1, int.MaxValue, ErrorMessage = "O id do lançamento deve ser maior que zero")]
    public int IdLancamentoReferencia { get; set; }
    
}