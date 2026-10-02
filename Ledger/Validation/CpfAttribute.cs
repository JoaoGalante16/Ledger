using System.ComponentModel.DataAnnotations;

namespace Ledger.Validation;

public class CpfAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var cpf = value as string;

        if (string.IsNullOrEmpty(cpf))
            return ValidationResult.Success;

        if (!CpfValido(cpf))
            return new ValidationResult("O cpf informado é inválido");

        return ValidationResult.Success;
    }

    private static bool CpfValido(string cpf)
    {
        if (cpf.Length != 11 || !cpf.All(c => c is >= '0' and <= '9'))
            return false;

        if (cpf.Distinct().Count() == 1)
            return false;

        var digitos = cpf.Select(c => c - '0').ToArray();

        return digitos[9] == CalcularDigito(digitos, 9)
               && digitos[10] == CalcularDigito(digitos, 10);
    }

    private static int CalcularDigito(int[] digitos, int quantidade)
    {
        var soma = 0;
        for (var i = 0; i < quantidade; i++)
            soma += digitos[i] * (quantidade + 1 - i);

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}