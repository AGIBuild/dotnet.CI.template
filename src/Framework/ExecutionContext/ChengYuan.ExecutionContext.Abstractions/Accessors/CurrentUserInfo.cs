namespace ChengYuan.ExecutionContext;

public sealed record CurrentUserInfo(string? Id, string? UserName, bool IsAuthenticated);
