using Ledger.Domain.Entities;
using Ledger.Domain.Repositories;

namespace Ledger.Application.Extensions;

public static class ContaRepositoryExtensions
{
    public static Task<Conta?> BuscarAcessivelAsync(this IContaRepository repository, int numeroConta, string idUsuario, bool eAdmin)
    {
        return repository.BuscarPorPkAsync(c => c.Numero == numeroConta && (eAdmin || c.IdUsuario == idUsuario));
    }
}
