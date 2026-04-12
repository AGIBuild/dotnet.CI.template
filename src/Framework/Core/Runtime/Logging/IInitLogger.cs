using Microsoft.Extensions.Logging;

namespace ChengYuan.Core.Logging;

public interface IInitLogger<out T> : ILogger<T>
{
    IReadOnlyList<InitLogEntry> Entries { get; }
}
