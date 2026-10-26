namespace Ledger.Domain.ValueObjects;

public record Cpf
{
    public string Value { get; }
    
    public Cpf(string value)
    {
        if (!CpfValido(value)) throw new ArgumentException("O cpf informado é inválido");
        Value = value;
    }
    
    public static bool CpfValido(string? cpf)
    {
        if (cpf is null || cpf.Length != 11 || !cpf.All(c => c is >= '0' and <= '9'))
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