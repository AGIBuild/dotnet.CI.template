namespace ChengYuan.MultiTenancy;

public interface ITenantScopeAccessPolicy
{
    bool CanAccess(Guid? tenantId);

    void EnsureCanAccess(Guid? tenantId);

    IReadOnlyList<TRecord> FilterAccessible<TRecord>(
        IEnumerable<TRecord> records,
        Func<TRecord, Guid?> tenantIdAccessor);
}
