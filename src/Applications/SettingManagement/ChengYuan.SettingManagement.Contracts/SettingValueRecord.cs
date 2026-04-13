using ChengYuan.Core;
using ChengYuan.Settings;

namespace ChengYuan.SettingManagement;

public sealed record SettingValueRecord
{
    public SettingValueRecord(string name, SettingScope scope, object? value, Guid? tenantId = null, string? userId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        ScopeValidator.ValidateScopeArguments(scope, tenantId, userId, "settings");

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
}
