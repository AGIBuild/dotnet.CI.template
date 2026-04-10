using ChengYuan.Authorization;

namespace ChengYuan.PermissionManagement;

public interface IPermissionGrantReader
{
    ValueTask<PermissionGrantRecord?> FindAsync(
        string name,
        PermissionScope scope,
        Guid? tenantId = null,
        string? userId = null,
        CancellationToken cancellationToken = default);

    ValueTask<IReadOnlyList<PermissionGrantRecord>> GetListAsync(CancellationToken cancellationToken = default);
}
