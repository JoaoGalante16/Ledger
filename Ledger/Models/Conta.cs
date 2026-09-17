using System.ComponentModel.DataAnnotations;

namespace Ledger.Models;

public class Conta
{
    [Key]
    public int Numero { get; set; }
    [Required]
    public string Nome { get; set; }
    [Required]
    public string Cpf { get; set; }

}
