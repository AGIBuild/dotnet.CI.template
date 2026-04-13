namespace ChengYuan.AspNetCore;

public sealed class ApiResponse
{
    public bool Success { get; init; }

    public object? Data { get; init; }

    public ApiErrorInfo? Error { get; init; }

    public static ApiResponse Ok(object? data = null) => new() { Success = true, Data = data };

    public static ApiResponse Fail(ApiErrorInfo error) => new() { Success = false, Error = error };
}
