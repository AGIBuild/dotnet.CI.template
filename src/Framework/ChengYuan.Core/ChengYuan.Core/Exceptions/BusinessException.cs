using ChengYuan.Core.Results;

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

    public ResultError ToResultError()
    {
        return new ResultError(ErrorCode!.Value, Message, ErrorType);
    }
}
