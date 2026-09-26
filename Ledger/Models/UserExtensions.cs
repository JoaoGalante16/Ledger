using System.Security.Claims;

namespace Ledger.Models;

public static class UserExtensions
{
    public static string ObterId(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier) ??
        throw new InvalidOperationException("Usuário autenticado sem a claim NameIdentifier.");

    public static bool EhAdmin(this ClaimsPrincipal user) =>
        user.IsInRole("Admin");
}