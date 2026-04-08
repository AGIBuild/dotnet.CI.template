namespace ChengYuan.ExecutionContext;

public interface ICurrentUser
{
    string? Id { get; }

    string? UserName { get; }

    bool IsAuthenticated { get; }
}
