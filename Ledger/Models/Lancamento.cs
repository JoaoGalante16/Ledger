using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ledger.Models;

public class Lancamento
{
    [Key]
    public int Id { get; set; }
    public int IdTransacao { get; set; }
    [ForeignKey("Conta")]
    public int NumeroConta { get; set; }
    public virtual Conta Conta { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Valor { get; set; }
    public DateTime DataTransacao { get; set; }
    public DateTime DataGravacao { get; set; }
    public string? Descricao { get; set; }
    [ForeignKey("LancamentoReferencia")]
    public int? IdLancamentoReferencia { get; set; }
    public virtual Lancamento? LancamentoReferencia { get; set; }

}
