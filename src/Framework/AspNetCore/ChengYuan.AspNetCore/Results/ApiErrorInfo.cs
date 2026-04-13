namespace ChengYuan.AspNetCore;

public sealed class ApiErrorInfo
{
    public required string Code { get; init; }

    public required string Message { get; init; }

    public string? Details { get; init; }
}
