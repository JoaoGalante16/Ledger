using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ledger.Models;

public class Conta
{
    [Key]
    public int Numero { get; set; }
    [Required]
    public string Nome { get; set; }
    [Required]
    public string Cpf { get; set; }
    [ForeignKey("Usuario")]
    public string IdUsuario { get; set; }
    public virtual Usuario Usuario { get; set; }
    public DateTime? DataEncerramento { get; set; }

}