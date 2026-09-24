using Ledger.Data;
using Ledger.Data.ContaDtos;
using Ledger.Data.LancamentoDtos;
using Ledger.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("LedgerConnection");
builder.Services.AddDbContext<LedgerContext>(opts => opts.UseLazyLoadingProxies().UseNpgsql(connectionString));

builder.Services.AddAutoMapper(cfg => { }, AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddScoped<IContaService,ContaService>();
builder.Services.AddScoped<ILancamentoService, LancamentoService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

