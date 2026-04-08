using ChengYuan.Authorization;

namespace ChengYuan.PermissionManagement;

public interface IPermissionGrantManager
{
    ValueTask SetAsync(PermissionGrantRecord record, CancellationToken cancellationToken = default);

    ValueTask RemoveAsync(
        string name,
        PermissionScope scope,
        Guid? tenantId = null,
        string? userId = null,
        CancellationToken cancellationToken = default);
}
