using System.ComponentModel.DataAnnotations;
using Ledger.Application.Validation;

namespace Ledger.Application.Dtos.Lancamentos;

public class CreateLancamentoDepositoOuSaque
{
    [Range(1,int.MaxValue, ErrorMessage = "O numero da conta é obrigatório e deve ser maior que zero")]
    public int NumeroConta { get; set; }
    [Range(0.01 , (double)decimal.MaxValue , ErrorMessage = "O valor deve ser maior que zero")]
    [MaximoDuasCasasDecimais]
    public decimal Valor { get; set; }
}