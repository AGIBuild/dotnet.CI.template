using System;
using Microsoft.Extensions.Logging;

namespace ChengYuan.ExceptionHandling;

public class BusinessException : Exception, IHasErrorCode, IHasErrorDetails, IHasLogLevel
{
    public BusinessException(
        string? code = null,
        string? message = null,
        string? details = null,
        Exception? innerException = null,
        LogLevel logLevel = LogLevel.Warning)
        : base(message, innerException)
    {
        Code = code;
        Details = details;
        LogLevel = logLevel;
    }

    public string? Code { get; }

    public string? Details { get; }

    public LogLevel LogLevel { get; }

    public BusinessException WithData(string name, object value)
    {
        Data[name] = value;
        return this;
    }
}
