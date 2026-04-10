using ChengYuan.Authorization;

namespace ChengYuan.PermissionManagement;

internal sealed class PermissionStoreUserGrantProvider(IPermissionGrantStore store) : IPermissionGrantProvider
{
    public string Name => "PermissionStoreUser";

    public int Order => 320;

    public async ValueTask<PermissionGrant?> GetOrNullAsync(PermissionDefinition definition, PermissionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (string.IsNullOrWhiteSpace(context.UserId))
        {
            return null;
        }

        var record = await store.FindAsync(definition.Name, PermissionScope.User, userId: context.UserId, cancellationToken: cancellationToken);
        return record is null ? null : new PermissionGrant(record.IsGranted, Name);
    }
}
