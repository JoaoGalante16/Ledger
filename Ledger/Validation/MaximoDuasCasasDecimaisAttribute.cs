using System.ComponentModel.DataAnnotations;

namespace Ledger.Validation;

public class MaximoDuasCasasDecimaisAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var valor = (decimal)value!;
        
        if (decimal.Round(valor, 2) == valor) return ValidationResult.Success;
        
        return new ValidationResult("O valor deve ter no máximo duas casas decimais");
    }
}