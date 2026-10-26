using Ledger.Application.Services;
using Ledger.Domain.Repositories;
using Ledger.Api.Handlers;
using Ledger.Infrastructure;
using Ledger.Infrastructure.Data;
using Ledger.Infrastructure.Data.Seeds;
using Ledger.Infrastructure.Identity;
using Ledger.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Cole o token recebido no login"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var connectionString = builder.Configuration.GetConnectionString("LedgerConnection");
builder.Services.AddDbContext<LedgerContext>(opts => opts.UseLazyLoadingProxies().UseNpgsql(connectionString));

builder.Services.AddIdentityApiEndpoints<Usuario>().AddRoles<IdentityRole>().AddEntityFrameworkStores<LedgerContext>();

builder.Services.AddAuthorization();

builder.Services.AddAutoMapper(cfg => { }, AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddScoped<IContaService,ContaService>();
builder.Services.AddScoped<ILancamentoService, LancamentoService>();

builder.Services.AddScoped<SeedAdmin>();
builder.Services.AddScoped<SeedContaCaixa>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

using (var scope = app.Services.CreateScope())
{
    var seedAdmin = scope.ServiceProvider.GetRequiredService<SeedAdmin>();
    await seedAdmin.CriarAsync(builder.Configuration["AdminEmail"]);

    var seedContaCaixa = scope.ServiceProvider.GetRequiredService<SeedContaCaixa>();
    await seedContaCaixa.CriarAsync();
}


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapGroup("auth").MapIdentityApi<Usuario>();
app.MapControllers();


app.Run();

