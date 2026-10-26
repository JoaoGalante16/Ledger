using System.ComponentModel.DataAnnotations;
using Ledger.Domain.ValueObjects;

namespace Ledger.Application.Validation;

public class CpfAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var cpf = value as string;

        if (string.IsNullOrEmpty(cpf))
            return ValidationResult.Success;

        if (!Cpf.CpfValido(cpf))
            return new ValidationResult("O cpf informado é inválido");

        return ValidationResult.Success;
    }
}