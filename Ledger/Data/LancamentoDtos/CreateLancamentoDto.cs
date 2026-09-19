using System.ComponentModel.DataAnnotations;

namespace Ledger.Data.LancamentoDtos;

public class CreateLancamentoDto : IValidatableObject
{
    [Required(ErrorMessage = "O numero da conta de origem é obrigatório")]
    public int NumeroContaOrigem { get; set; }
    [Required(ErrorMessage = "O numero da conta de destino é obrigatório")]
    public int NumeroContaDestino { get; set; }
    [Required(ErrorMessage = "O valor é obrigatório")]
    [Range(0.01 , (double)decimal.MaxValue , ErrorMessage = "O valor deve ser maior que zero")]
    public decimal Valor { get; set; }
    public DateTime DataTransacao { get; set; } = DateTime.UtcNow;
    [StringLength(250, ErrorMessage = "A descrição não pode passar de 250 caracteres")]
    public string? Descricao { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (NumeroContaOrigem == NumeroContaDestino)
        {
            yield return new ValidationResult("A conta de origem e a conta de destino não podem ser a mesma.",
                new[] { nameof(NumeroContaDestino) });
        }
    }
}