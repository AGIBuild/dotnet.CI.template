namespace ChengYuan.Core.Exceptions;

public readonly record struct ErrorCode
{
    public ErrorCode(string value)
    {
        Value = Check.NotNullOrWhiteSpace(value, nameof(value));
    }

    public string Value { get; }

    public override string ToString()
    {
        return Value;
    }

    public static implicit operator string(ErrorCode errorCode)
    {
        return errorCode.Value;
    }

    public static explicit operator ErrorCode(string value)
    {
        return new ErrorCode(value);
    }
}
