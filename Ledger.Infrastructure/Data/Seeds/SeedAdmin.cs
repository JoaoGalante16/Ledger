using Ledger.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace Ledger.Infrastructure.Data.Seeds;

public class SeedAdmin
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<Usuario> _userManager;

    public SeedAdmin(RoleManager<IdentityRole> roleManager, UserManager<Usuario> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task CriarAsync(string? adminEmail)
    {
        if (!await _roleManager.RoleExistsAsync("Admin"))
            await _roleManager.CreateAsync(new IdentityRole("Admin"));

        if (!string.IsNullOrEmpty(adminEmail))
        {
            var admin = await _userManager.FindByEmailAsync(adminEmail);
            if (admin is not null && !await _userManager.IsInRoleAsync(admin, "Admin"))
                await _userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}