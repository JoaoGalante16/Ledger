using System.ComponentModel.DataAnnotations;
using Ledger.Validation;

namespace Ledger.Data.Dtos.LancamentoDtos;

public class CreateLancamentoDto : IValidatableObject
{
    [Range(1,int.MaxValue, ErrorMessage = "O numero da conta de origem é obrigatório e deve ser maior que zero")]
    public int NumeroContaOrigem { get; set; }
    [Range(1,int.MaxValue, ErrorMessage = "O numero da conta de destino é obrigatório e deve ser maior que zero")]
    public int NumeroContaDestino { get; set; }
    [Range(0.01 , (double)decimal.MaxValue , ErrorMessage = "O valor deve ser maior que zero")]
    [MaximoDuasCasasDecimais]
    public decimal Valor { get; set; }
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