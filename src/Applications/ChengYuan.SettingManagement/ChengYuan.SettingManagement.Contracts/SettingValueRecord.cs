using ChengYuan.Settings;

namespace ChengYuan.SettingManagement;

public sealed class SettingValueRecord
{
    public SettingValueRecord(string name, SettingScope scope, object? value, Guid? tenantId = null, string? userId = null)
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

    public SettingScope Scope { get; }

    public object? Value { get; }

    public Guid? TenantId { get; }

    public string? UserId { get; }

    private static void ValidateScopeArguments(SettingScope scope, Guid? tenantId, string? userId)
    {
        switch (scope)
        {
            case SettingScope.Global:
                if (tenantId is not null || !string.IsNullOrWhiteSpace(userId))
                {
                    throw new ArgumentException("Global settings cannot specify tenant or user values.");
                }

                break;

            case SettingScope.Tenant:
                if (tenantId is null || tenantId == Guid.Empty)
                {
                    throw new ArgumentException("Tenant settings must specify a non-empty tenant id.", nameof(tenantId));
                }

                if (!string.IsNullOrWhiteSpace(userId))
                {
                    throw new ArgumentException("Tenant settings cannot specify a user id.", nameof(userId));
                }

                break;

            case SettingScope.User:
                if (string.IsNullOrWhiteSpace(userId))
                {
                    throw new ArgumentException("User settings must specify a user id.", nameof(userId));
                }

                if (tenantId is not null)
                {
                    throw new ArgumentException("User settings are keyed by user id only and cannot specify a tenant id.", nameof(tenantId));
                }

                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(scope), scope, "Unsupported setting scope.");
        }
    }
}
