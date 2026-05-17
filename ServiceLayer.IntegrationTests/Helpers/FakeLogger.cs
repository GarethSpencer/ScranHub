using Microsoft.Extensions.Logging;

namespace ServiceLayer.IntegrationTests.Helpers;

public class FakeLogger<T> : ILogger<T> where T : class
{
    public List<LogEntry> Entries = [];

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Entries.Add(new LogEntry(logLevel, formatter(state, exception)));
    }
}

public record LogEntry(LogLevel Level, string Message);
