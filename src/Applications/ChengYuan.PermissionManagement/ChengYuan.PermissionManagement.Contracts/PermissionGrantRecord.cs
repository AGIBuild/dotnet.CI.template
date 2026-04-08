using ChengYuan.Authorization;

namespace ChengYuan.PermissionManagement;

public sealed class PermissionGrantRecord
{
    public PermissionGrantRecord(string name, PermissionScope scope, bool isGranted, Guid? tenantId = null, string? userId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        ValidateScopeArguments(scope, tenantId, userId);

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

    private static void ValidateScopeArguments(PermissionScope scope, Guid? tenantId, string? userId)
    {
        switch (scope)
        {
            case PermissionScope.Global:
                if (tenantId is not null || !string.IsNullOrWhiteSpace(userId))
                {
                    throw new ArgumentException("Global grants cannot specify tenant or user values.");
                }

                break;

            case PermissionScope.Tenant:
                if (tenantId is null || tenantId == Guid.Empty)
                {
                    throw new ArgumentException("Tenant grants must specify a non-empty tenant id.", nameof(tenantId));
                }

                if (!string.IsNullOrWhiteSpace(userId))
                {
                    throw new ArgumentException("Tenant grants cannot specify a user id.", nameof(userId));
                }

                break;

            case PermissionScope.User:
                if (string.IsNullOrWhiteSpace(userId))
                {
                    throw new ArgumentException("User grants must specify a user id.", nameof(userId));
                }

                if (tenantId is not null)
                {
                    throw new ArgumentException("User grants are keyed by user id only and cannot specify a tenant id.", nameof(tenantId));
                }

                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(scope), scope, "Unsupported permission scope.");
        }
    }
}
