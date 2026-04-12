using Microsoft.Extensions.Logging;

namespace ChengYuan.Core.Logging;

public sealed record InitLogEntry(
    string CategoryName,
    LogLevel LogLevel,
    EventId EventId,
    string Message,
    Exception? Exception);
