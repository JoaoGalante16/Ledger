
namespace Ledger.Data.LancamentoDtos;

public class ReadLancamentoDto
{
    public int Id { get; set; }
    public int IdTransacao { get; set; }
    public int NumeroConta { get; set; }
    public decimal Valor { get; set; }
    public DateTime DataTransacao { get; set; }
    public DateTime DataGravacao { get; set; }
    public string? Descricao { get; set; }
    public int? IdLancamentoReferencia { get; set; }
}