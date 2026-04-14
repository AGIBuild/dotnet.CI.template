namespace ChengYuan.ExceptionHandling;

public sealed class ErrorValidationInfo
{
    public ErrorValidationInfo(string? member = null, string? message = null)
    {
        Member = member;
        Message = message;
    }

    public string? Member { get; set; }

    public string? Message { get; set; }
}
