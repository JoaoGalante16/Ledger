using Ledger.Domain.Entities;

namespace Ledger.Application.Extensions;

public static class ContaExtensions
{
    public static bool PodeSerAcessadaPor(this Conta conta, string idUsuario, bool eAdmin)
    {
        return eAdmin || conta.IdUsuario == idUsuario;
    }
}
