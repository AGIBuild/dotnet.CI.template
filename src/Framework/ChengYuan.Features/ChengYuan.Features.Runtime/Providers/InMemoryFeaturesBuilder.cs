using System;
using System.Collections.Generic;

namespace ChengYuan.Features;

public sealed class InMemoryFeaturesBuilder
{
    internal Dictionary<string, object?> GlobalValues { get; } = new(StringComparer.Ordinal);

    internal Dictionary<(Guid TenantId, string Name), object?> TenantValues { get; } = new();

    internal Dictionary<(string UserId, string Name), object?> UserValues { get; } = new();

    public InMemoryFeaturesBuilder SetGlobal<T>(string name, T value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        GlobalValues[name] = value;
        return this;
    }

    public InMemoryFeaturesBuilder SetTenant<T>(string name, Guid tenantId, T value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant id cannot be empty.", nameof(tenantId));
        }

        TenantValues[(tenantId, name)] = value;
        return this;
    }

    public InMemoryFeaturesBuilder SetUser<T>(string name, string userId, T value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        UserValues[(userId, name)] = value;
        return this;
    }
}
