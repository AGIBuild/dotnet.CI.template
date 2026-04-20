using ChengYuan.Authorization;

namespace ChengYuan.PermissionManagement;

internal sealed class PermissionStoreUserGrantProvider(IPermissionGrantStore store) : IPermissionGrantProvider
{
    public string Name => "PermissionStoreUser";

    public int Order => 320;

    public async ValueTask<PermissionGrantResult> CheckAsync(PermissionDefinition definition, PermissionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (string.IsNullOrWhiteSpace(context.UserId))
        {
            return PermissionGrantResult.Undefined;
        }

        var record = await store.FindAsync(definition.Name, PermissionScope.User, userId: context.UserId, cancellationToken: cancellationToken);
        if (record is null)
        {
            return PermissionGrantResult.Undefined;
        }

        return record.IsGranted ? PermissionGrantResult.Granted : PermissionGrantResult.Prohibited;
    }
}
