using ChengYuan.Authorization;

namespace ChengYuan.PermissionManagement;

internal sealed class PermissionStoreGlobalGrantProvider(IPermissionGrantStore store) : IPermissionGrantProvider
{
    public string Name => "PermissionStoreGlobal";

    public int Order => 120;

    public async ValueTask<PermissionGrant?> GetOrNullAsync(PermissionDefinition definition, PermissionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var record = await store.FindAsync(definition.Name, PermissionScope.Global, cancellationToken: cancellationToken);
        return record is null ? null : new PermissionGrant(record.IsGranted, Name);
    }
}
