using Microsoft.Extensions.Logging;

namespace Ledger.Tests.Unitarios.Suporte;

public record RegistroDeLog(LogLevel Nivel, string Mensagem, Dictionary<string, object?> Campos);

public class LoggerDeTeste<T> : ILogger<T>
{
    public List<RegistroDeLog> Registros { get; } = new();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var campos = state as IEnumerable<KeyValuePair<string, object?>> ?? [];
        Registros.Add(new RegistroDeLog(logLevel, formatter(state, exception), campos.ToDictionary(c => c.Key, c => c.Value)));
    }
}
