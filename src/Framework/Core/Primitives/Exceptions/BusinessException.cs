using ChengYuan.Core.Results;
using Microsoft.Extensions.Logging;

namespace ChengYuan.Core.Exceptions;

public sealed class BusinessException : ChengYuanException
{
    public BusinessException(string message, ErrorCode errorCode, ResultErrorType errorType = ResultErrorType.Failure)
        : base(message, errorCode)
    {
        ErrorType = errorType;
    }

    public BusinessException(string message, ErrorCode errorCode, Exception innerException, ResultErrorType errorType = ResultErrorType.Failure)
        : base(message, errorCode, innerException)
    {
        ErrorType = errorType;
    }

    public ResultErrorType ErrorType { get; }

    public string? Details { get; init; }

    public LogLevel LogLevel { get; init; } = LogLevel.Warning;

    public BusinessException WithData(string name, object value)
    {
        Data[name] = value;
        return this;
    }

    public ResultError ToResultError()
    {
        return new ResultError(ErrorCode!.Value, Message, ErrorType);
    }
}
