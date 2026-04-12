using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace ChengYuan.Core.Logging;

public sealed class DefaultInitLoggerFactory : IInitLoggerFactory
{
    private readonly ConcurrentDictionary<Type, IInitLoggerEntries> _loggers = new();

    public IInitLogger<T> Create<T>()
    {
        var logger = _loggers.GetOrAdd(typeof(T), static _ => new DefaultInitLogger<T>());
        return (IInitLogger<T>)logger;
    }

    public IReadOnlyList<InitLogEntry> GetAllEntries()
    {
        return _loggers.Values
            .SelectMany(static logger => logger.Entries)
            .ToArray();
    }

    public void ClearAllEntries()
    {
        _loggers.Clear();
    }
}
