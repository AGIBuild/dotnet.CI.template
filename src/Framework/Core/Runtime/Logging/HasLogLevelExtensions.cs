using Microsoft.Extensions.Logging;

namespace ChengYuan.Core.Logging;

public static class HasLogLevelExtensions
{
    public static TException WithLogLevel<TException>(this TException exception, LogLevel logLevel)
        where TException : Exception, IHasLogLevel
    {
        exception.LogLevel = logLevel;
        return exception;
    }
}
