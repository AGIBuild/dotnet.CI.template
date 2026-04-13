namespace ChengYuan.ExceptionHandling;

public sealed class ErrorResponse(ErrorInfo error)
{
    public ErrorInfo Error { get; } = error;
}
