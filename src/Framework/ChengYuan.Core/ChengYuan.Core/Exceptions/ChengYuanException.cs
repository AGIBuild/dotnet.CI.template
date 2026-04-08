namespace ChengYuan.Core.Exceptions;

public class ChengYuanException : Exception
{
    public ChengYuanException(string message)
        : base(Check.NotNullOrWhiteSpace(message, nameof(message)))
    {
    }

    public ChengYuanException(string message, Exception innerException)
        : base(Check.NotNullOrWhiteSpace(message, nameof(message)), innerException)
    {
        ArgumentNullException.ThrowIfNull(innerException);
    }

    public ChengYuanException(string message, ErrorCode errorCode)
        : this(message)
    {
        ErrorCode = errorCode;
    }

    public ChengYuanException(string message, ErrorCode errorCode, Exception innerException)
        : this(message, innerException)
    {
        ErrorCode = errorCode;
    }

    public ErrorCode? ErrorCode { get; }
}
