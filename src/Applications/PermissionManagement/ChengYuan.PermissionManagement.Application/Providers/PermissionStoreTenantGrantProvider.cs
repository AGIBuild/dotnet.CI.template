using ChengYuan.Authorization;

namespace ChengYuan.PermissionManagement;

internal sealed class PermissionStoreTenantGrantProvider(IPermissionGrantStore store) : IPermissionGrantProvider
{
    public string Name => "PermissionStoreTenant";

    public int Order => 220;

    public async ValueTask<PermissionGrant?> GetOrNullAsync(PermissionDefinition definition, PermissionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (context.TenantId is not Guid tenantId)
        {
            return null;
        }

        var record = await store.FindAsync(definition.Name, PermissionScope.Tenant, tenantId, cancellationToken: cancellationToken);
        return record is null ? null : new PermissionGrant(record.IsGranted, Name);
    }
}
