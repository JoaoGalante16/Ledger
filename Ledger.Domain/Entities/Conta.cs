
using Ledger.Domain.ValueObjects;

namespace Ledger.Domain.Entities;

public class Conta
{
    public int Numero { get; set; }
    public string Nome { get; set; }
    public Cpf Cpf { get; set; }
    public string IdUsuario { get; set; }
    public DateTime? DataEncerramento { get; set; }

}