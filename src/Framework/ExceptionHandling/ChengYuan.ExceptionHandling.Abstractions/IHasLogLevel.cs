using Microsoft.Extensions.Logging;

namespace ChengYuan.ExceptionHandling;

public interface IHasLogLevel
{
    LogLevel LogLevel { get; }
}
