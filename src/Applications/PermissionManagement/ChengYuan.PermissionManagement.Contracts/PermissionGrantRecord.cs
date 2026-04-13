using ChengYuan.Authorization;
using ChengYuan.Core;

namespace ChengYuan.PermissionManagement;

public sealed record PermissionGrantRecord
{
    public PermissionGrantRecord(string name, PermissionScope scope, bool isGranted, Guid? tenantId = null, string? userId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        ScopeValidator.ValidateScopeArguments(scope, tenantId, userId, "grants");

        Name = name;
        Scope = scope;
        IsGranted = isGranted;
        TenantId = tenantId;
        UserId = userId;
    }

    public string Name { get; }

    public PermissionScope Scope { get; }

    public bool IsGranted { get; }

    public Guid? TenantId { get; }

    public string? UserId { get; }
}
