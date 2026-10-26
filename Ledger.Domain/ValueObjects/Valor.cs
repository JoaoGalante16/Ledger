namespace Ledger.Domain.ValueObjects;

public record Valor
{
    public decimal Value { get; }

    public Valor(decimal value)
    {
        if (decimal.Round(value, 2) != value)
            throw new ArgumentException("O valor deve ter no máximo duas casas decimais");

        Value = value;  
    }
}