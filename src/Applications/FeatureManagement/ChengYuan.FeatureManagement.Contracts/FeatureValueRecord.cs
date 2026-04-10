using ChengYuan.Features;

namespace ChengYuan.FeatureManagement;

public sealed class FeatureValueRecord
{
    public FeatureValueRecord(string name, FeatureScope scope, object? value, Guid? tenantId = null, string? userId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        ValidateScopeArguments(scope, tenantId, userId);

        Name = name;
        Scope = scope;
        Value = value;
        TenantId = tenantId;
        UserId = userId;
    }

    public string Name { get; }

    public FeatureScope Scope { get; }

    public object? Value { get; }

    public Guid? TenantId { get; }

    public string? UserId { get; }

    private static void ValidateScopeArguments(FeatureScope scope, Guid? tenantId, string? userId)
    {
        switch (scope)
        {
            case FeatureScope.Global:
                if (tenantId is not null || !string.IsNullOrWhiteSpace(userId))
                {
                    throw new ArgumentException("Global features cannot specify tenant or user values.");
                }

                break;

            case FeatureScope.Tenant:
                if (tenantId is null || tenantId == Guid.Empty)
                {
                    throw new ArgumentException("Tenant features must specify a non-empty tenant id.", nameof(tenantId));
                }

                if (!string.IsNullOrWhiteSpace(userId))
                {
                    throw new ArgumentException("Tenant features cannot specify a user id.", nameof(userId));
                }

                break;

            case FeatureScope.User:
                if (string.IsNullOrWhiteSpace(userId))
                {
                    throw new ArgumentException("User features must specify a user id.", nameof(userId));
                }

                if (tenantId is not null)
                {
                    throw new ArgumentException("User features are keyed by user id only and cannot specify a tenant id.", nameof(tenantId));
                }

                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(scope), scope, "Unsupported feature scope.");
        }
    }
}
