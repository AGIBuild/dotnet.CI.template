using ChengYuan.Authorization;

namespace ChengYuan.PermissionManagement;

internal sealed class PermissionStoreTenantGrantProvider(IPermissionGrantStore store) : IPermissionGrantProvider
{
    public string Name => "PermissionStoreTenant";

    public int Order => 220;

    public async ValueTask<PermissionGrantResult> CheckAsync(PermissionDefinition definition, PermissionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (context.TenantId is not Guid tenantId)
        {
            return PermissionGrantResult.Undefined;
        }

        var record = await store.FindAsync(definition.Name, PermissionScope.Tenant, tenantId, cancellationToken: cancellationToken);
        if (record is null)
        {
            return PermissionGrantResult.Undefined;
        }

        return record.IsGranted ? PermissionGrantResult.Granted : PermissionGrantResult.Prohibited;
    }
}
