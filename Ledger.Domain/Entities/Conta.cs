
namespace Ledger.Domain.Entities;

public class Conta
{
    public int Numero { get; set; }
    public string Nome { get; set; }
    public string Cpf { get; set; }
    public string IdUsuario { get; set; }
    public DateTime? DataEncerramento { get; set; }

}