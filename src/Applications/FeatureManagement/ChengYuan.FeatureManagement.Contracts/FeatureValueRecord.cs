using ChengYuan.Core;
using ChengYuan.Features;

namespace ChengYuan.FeatureManagement;

public sealed record FeatureValueRecord
{
    public FeatureValueRecord(string name, FeatureScope scope, object? value, Guid? tenantId = null, string? userId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        ScopeValidator.ValidateScopeArguments(scope, tenantId, userId, "features");

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
}
