using ChengYuan.Authorization;

namespace ChengYuan.PermissionManagement;

public sealed class PermissionGrantManager(IPermissionGrantStore store) : IPermissionGrantManager
{
    public ValueTask SetAsync(PermissionGrantRecord record, CancellationToken cancellationToken = default)
    {
        return store.SetAsync(record, cancellationToken);
    }

    public ValueTask RemoveAsync(
        string name,
        PermissionScope scope,
        Guid? tenantId = null,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        return store.RemoveAsync(name, scope, tenantId, userId, cancellationToken);
    }
}
