using ChengYuan.MultiTenancy;

namespace ChengYuan.AuditLogging;

public sealed class AuditLogManager(
    IAuditLogReader reader,
    ITenantScopeAccessPolicy tenantScopeAccessPolicy) : IAuditLogManager
{
    public async ValueTask<IReadOnlyList<AuditLogRecord>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var records = await reader.GetListAsync(cancellationToken);
        return tenantScopeAccessPolicy.FilterAccessible(records, record => record.TenantId);
    }
}
