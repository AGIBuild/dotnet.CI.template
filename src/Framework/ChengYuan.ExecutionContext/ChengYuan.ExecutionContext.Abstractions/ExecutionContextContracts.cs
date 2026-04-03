namespace ChengYuan.ExecutionContext;

public sealed record CurrentUserInfo(string? Id, string? UserName, bool IsAuthenticated);

public interface ICurrentUser
{
    string? Id { get; }

    string? UserName { get; }

    bool IsAuthenticated { get; }
}

public interface ICurrentUserAccessor : ICurrentUser
{
    IDisposable Change(CurrentUserInfo? currentUser);
}

public interface ICurrentCorrelation
{
    string CorrelationId { get; }
}

public interface ICurrentCorrelationAccessor : ICurrentCorrelation
{
    IDisposable Change(string? correlationId);
}
