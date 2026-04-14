using ChengYuan.Core.Data.Auditing;

namespace ChengYuan.Auditing;

internal sealed class AuditScopeEntityChangeCollector(IAuditScopeAccessor scopeAccessor) : IEntityChangeCollector
{
    public bool IsActive => scopeAccessor.Current is not null;

    public void Collect(EntityChangeInfo changeInfo)
    {
        ArgumentNullException.ThrowIfNull(changeInfo);

        scopeAccessor.Current?.Entry.EntityChanges.Add(changeInfo);
    }
}
