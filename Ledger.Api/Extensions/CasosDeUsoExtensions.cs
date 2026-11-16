using Ledger.Application.UseCases.Contas;
using Ledger.Application.UseCases.Lancamentos;

namespace Ledger.Api.Extensions;

public static class CasosDeUsoExtensions
{
    public static IServiceCollection AddCasosDeUso(this IServiceCollection services)
    {
        services.AddScoped<CriarConta>();
        services.AddScoped<ListarContas>();
        services.AddScoped<BuscarConta>();
        services.AddScoped<AtualizarConta>();
        services.AddScoped<RemoverConta>();
        services.AddScoped<EncerrarConta>();

        services.AddScoped<Transferir>();
        services.AddScoped<Depositar>();
        services.AddScoped<Sacar>();
        services.AddScoped<CorrigirLancamento>();
        services.AddScoped<ListarLancamentos>();
        services.AddScoped<BuscarLancamento>();
        services.AddScoped<BuscarParDaTransacao>();

        return services;
    }
}
