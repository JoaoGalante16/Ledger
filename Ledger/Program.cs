using Ledger.Data;
using Ledger.Data.ContaDtos;
using Ledger.Data.LancamentoDtos;
using Ledger.Models;
using Ledger.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("LedgerConnection");
builder.Services.AddDbContext<LedgerContext>(opts => opts.UseLazyLoadingProxies().UseNpgsql(connectionString));

builder.Services.AddIdentityApiEndpoints<Usuario>().AddRoles<IdentityRole>().AddEntityFrameworkStores<LedgerContext>();

builder.Services.AddAuthorization();

builder.Services.AddAutoMapper(cfg => { }, AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddScoped<IContaService,ContaService>();
builder.Services.AddScoped<ILancamentoService, LancamentoService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();

    if (!await roleManager.RoleExistsAsync("Admin"))
        await roleManager.CreateAsync(new IdentityRole("Admin"));

    var adminEmail = builder.Configuration["AdminEmail"];
    if (!string.IsNullOrEmpty(adminEmail))
    {
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is not null && !await userManager.IsInRoleAsync(admin, "Admin"))
            await userManager.AddToRoleAsync(admin, "Admin");
    }
}


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapGroup("auth").MapIdentityApi<Usuario>();
app.MapControllers();


app.Run();

