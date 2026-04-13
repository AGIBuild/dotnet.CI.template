using System.Collections.Generic;

namespace ChengYuan.ExceptionHandling;

public sealed class ErrorInfo
{
    public ErrorInfo(string? code = null, string? message = null, string? details = null)
    {
        Code = code;
        Message = message;
        Details = details;
    }

    public string? Code { get; set; }

    public string? Message { get; set; }

    public string? Details { get; set; }

    public IReadOnlyList<ErrorValidationInfo>? ValidationErrors { get; set; }
}
