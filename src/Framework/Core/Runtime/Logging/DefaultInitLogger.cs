using Microsoft.Extensions.Logging;

namespace ChengYuan.Core.Logging;

internal sealed class DefaultInitLogger<T> : IInitLogger<T>, IInitLoggerEntries
{
    private readonly List<InitLogEntry> _entries = [];
    private readonly string _categoryName = typeof(T).FullName ?? typeof(T).Name;

    public IReadOnlyList<InitLogEntry> Entries => _entries;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        _entries.Add(new InitLogEntry(
            _categoryName,
            logLevel,
            eventId,
            formatter(state, exception),
            exception));
    }

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();

        public void Dispose()
        {
        }
    }
}
