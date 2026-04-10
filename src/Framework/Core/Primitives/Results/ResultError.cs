namespace ChengYuan.Core.Results;

public sealed record ResultError(string Code, string Message, ResultErrorType Type = ResultErrorType.Failure)
{
    public static ResultError None { get; } = new(string.Empty, string.Empty);

    public bool IsNone => string.IsNullOrEmpty(Code) && string.IsNullOrEmpty(Message);

    public static ResultError Validation(string code, string message) => new(code, message, ResultErrorType.Validation);

    public static ResultError Conflict(string code, string message) => new(code, message, ResultErrorType.Conflict);

    public static ResultError NotFound(string code, string message) => new(code, message, ResultErrorType.NotFound);

    public static ResultError Unauthorized(string code, string message) => new(code, message, ResultErrorType.Unauthorized);

    public static ResultError Forbidden(string code, string message) => new(code, message, ResultErrorType.Forbidden);

    public static ResultError Unexpected(string code, string message) => new(code, message, ResultErrorType.Unexpected);
}
