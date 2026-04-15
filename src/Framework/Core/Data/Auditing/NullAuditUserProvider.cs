namespace ChengYuan.Core.Data.Auditing;

public sealed class NullAuditUserProvider : IAuditUserProvider
{
    public static NullAuditUserProvider Instance { get; } = new();

    public string? UserId => null;
}
