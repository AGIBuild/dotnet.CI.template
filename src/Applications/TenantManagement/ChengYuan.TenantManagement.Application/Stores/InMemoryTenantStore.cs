namespace ChengYuan.TenantManagement;

public sealed class InMemoryTenantStore : ITenantStore
{
    private readonly object _sync = new();
    private readonly Dictionary<Guid, TenantRecord> _tenantsById = new();
    private readonly Dictionary<string, Guid> _tenantIdsByName = new(StringComparer.OrdinalIgnoreCase);

    public ValueTask<TenantRecord?> FindByIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant id cannot be empty.", nameof(tenantId));
        }

        lock (_sync)
        {
            return ValueTask.FromResult(_tenantsById.GetValueOrDefault(tenantId));
        }
    }

    public ValueTask<TenantRecord?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        lock (_sync)
        {
            return ValueTask.FromResult(
                _tenantIdsByName.TryGetValue(name, out var tenantId)
                    ? _tenantsById.GetValueOrDefault(tenantId)
                    : null);
        }
    }

    public ValueTask<IReadOnlyList<TenantRecord>> GetListAsync(CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            var tenants = _tenantsById.Values
                .OrderBy(tenant => tenant.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(tenant => tenant.Id)
                .ToArray();

            return ValueTask.FromResult<IReadOnlyList<TenantRecord>>(tenants);
        }
    }

    public ValueTask SetAsync(TenantRecord tenant, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tenant);

        lock (_sync)
        {
            if (_tenantIdsByName.TryGetValue(tenant.Name, out var existingTenantId) && existingTenantId != tenant.Id)
            {
                throw new InvalidOperationException($"A tenant named '{tenant.Name}' already exists.");
            }

            if (_tenantsById.TryGetValue(tenant.Id, out var existingTenant))
            {
                _tenantIdsByName.Remove(existingTenant.Name);
            }

            _tenantsById[tenant.Id] = tenant;
            _tenantIdsByName[tenant.Name] = tenant.Id;
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask RemoveAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant id cannot be empty.", nameof(tenantId));
        }

        lock (_sync)
        {
            if (_tenantsById.Remove(tenantId, out var removedTenant))
            {
                _tenantIdsByName.Remove(removedTenant.Name);
            }
        }

        return ValueTask.CompletedTask;
    }
}
