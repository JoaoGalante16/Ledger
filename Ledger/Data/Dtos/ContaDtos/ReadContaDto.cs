namespace Ledger.Data.Dtos.ContaDtos;

public class ReadContaDto
{
    public int Numero { get; set; }
    public string Nome { get; set; }
    public string Cpf { get; set; }
    public decimal Saldo { get; set; }
    public DateTime? DataEncerramento { get; set; }
}