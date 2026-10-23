using System.ComponentModel.DataAnnotations;
using Ledger.Application.Validation;

namespace Ledger.Application.Dtos.Contas;

public class CreateContaDto
{
    [Required(ErrorMessage = "O nome da conta é obrigatório")]
    [StringLength(250, ErrorMessage = "O nome não pode ter mais de 250 caracteres")]
    public string Nome { get; set; }
    [Required(ErrorMessage = "O cpf da conta é obrigatório")]
    [Cpf]
    public string Cpf { get; set; }
}