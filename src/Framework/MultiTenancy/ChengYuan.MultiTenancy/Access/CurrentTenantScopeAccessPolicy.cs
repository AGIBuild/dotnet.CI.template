namespace ChengYuan.MultiTenancy;

internal sealed class CurrentTenantScopeAccessPolicy(ICurrentTenant currentTenant) : ITenantScopeAccessPolicy
{
    public bool CanAccess(Guid? tenantId)
    {
        if (currentTenant.Id is not Guid currentTenantId)
        {
            return true;
        }

        return tenantId == currentTenantId;
    }

    public void EnsureCanAccess(Guid? tenantId)
    {
        if (!CanAccess(tenantId))
        {
            throw new UnauthorizedAccessException("The current tenant context cannot access the requested tenant scope.");
        }
    }

    public IReadOnlyList<TRecord> FilterAccessible<TRecord>(
        IEnumerable<TRecord> records,
        Func<TRecord, Guid?> tenantIdAccessor)
    {
        ArgumentNullException.ThrowIfNull(records);
        ArgumentNullException.ThrowIfNull(tenantIdAccessor);

        if (currentTenant.Id is not Guid currentTenantId)
        {
            return records.ToArray();
        }

        return records
            .Where(record => tenantIdAccessor(record) == currentTenantId)
            .ToArray();
    }
}
