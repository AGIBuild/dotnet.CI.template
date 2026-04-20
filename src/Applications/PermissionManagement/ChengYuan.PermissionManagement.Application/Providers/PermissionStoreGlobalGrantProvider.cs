using ChengYuan.Authorization;

namespace ChengYuan.PermissionManagement;

internal sealed class PermissionStoreGlobalGrantProvider(IPermissionGrantStore store) : IPermissionGrantProvider
{
    public string Name => "PermissionStoreGlobal";

    public int Order => 120;

    public async ValueTask<PermissionGrantResult> CheckAsync(PermissionDefinition definition, PermissionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var record = await store.FindAsync(definition.Name, PermissionScope.Global, cancellationToken: cancellationToken);
        if (record is null)
        {
            return PermissionGrantResult.Undefined;
        }

        return record.IsGranted ? PermissionGrantResult.Granted : PermissionGrantResult.Prohibited;
    }
}
