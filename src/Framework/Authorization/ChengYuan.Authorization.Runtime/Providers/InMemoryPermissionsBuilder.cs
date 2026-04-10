using System;
using System.Collections.Generic;

namespace ChengYuan.Authorization;

public sealed class InMemoryPermissionsBuilder
{
    internal Dictionary<string, bool> GlobalValues { get; } = new(StringComparer.Ordinal);

    internal Dictionary<(Guid TenantId, string Name), bool> TenantValues { get; } = new();

    internal Dictionary<(string UserId, string Name), bool> UserValues { get; } = new();

    public InMemoryPermissionsBuilder SetGlobal(string name, bool isGranted)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        GlobalValues[name] = isGranted;
        return this;
    }

    public InMemoryPermissionsBuilder SetTenant(string name, Guid tenantId, bool isGranted)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant id cannot be empty.", nameof(tenantId));
        }

        TenantValues[(tenantId, name)] = isGranted;
        return this;
    }

    public InMemoryPermissionsBuilder SetUser(string name, string userId, bool isGranted)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        UserValues[(userId, name)] = isGranted;
        return this;
    }
}
