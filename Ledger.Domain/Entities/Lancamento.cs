
using Ledger.Domain.Enums;
using Ledger.Domain.ValueObjects;

namespace Ledger.Domain.Entities;

public class Lancamento
{
    public int Id { get; set; }
    public int IdTransacao { get; set; }
    public int NumeroConta { get; set; }
    public virtual Conta Conta { get; set; }
    public Valor Valor { get; set; }
    public DateTime DataTransacao { get; set; }
    public DateTime DataGravacao { get; set; }
    public TipoOperacao TipoOperacao { get; set; }
    public string? Descricao { get; set; }
    public int? IdLancamentoReferencia { get; set; }
    public virtual Lancamento? LancamentoReferencia { get; set; }

}
