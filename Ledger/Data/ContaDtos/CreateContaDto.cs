using System.ComponentModel.DataAnnotations;

namespace Ledger.Data.ContaDtos;

public class CreateContaDto
{
    [Required(ErrorMessage = "O nome da conta é obrigatório")]
    [StringLength(250, ErrorMessage = "O nome não pode ter mais de 250 caracteres")]
    public string Nome { get; set; }
    [Required(ErrorMessage = "O cpf da conta é obrigatório")]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "O cpf deve conter 11 dígitos numéricos")]
    public string Cpf { get; set; }
}