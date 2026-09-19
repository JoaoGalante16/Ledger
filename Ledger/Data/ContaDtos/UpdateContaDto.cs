using System.ComponentModel.DataAnnotations;

namespace Ledger.Data.ContaDtos;

public class UpdateContaDto
{
    [Required(ErrorMessage = "O nome da conta é obrigatório")]
    [StringLength(250, ErrorMessage = "O nome não pode ter mais de 250 caracteres")]
    public string Nome { get; set; }
}