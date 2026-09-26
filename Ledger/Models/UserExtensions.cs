using System.Security.Claims;

namespace Ledger.Models;

public static class UserExtensions
{
    public static string? ObterId(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier);

    public static bool EhAdmin(this ClaimsPrincipal user) =>
        user.IsInRole("Admin");
}