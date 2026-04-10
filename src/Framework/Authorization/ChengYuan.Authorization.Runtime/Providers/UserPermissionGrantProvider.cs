using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Authorization;

internal sealed class UserPermissionGrantProvider : IPermissionGrantProvider
{
    public string Name => "User";

    public int Order => 300;

    public ValueTask<PermissionGrant?> GetOrNullAsync(PermissionDefinition definition, PermissionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return ValueTask.FromResult<PermissionGrant?>(null);
    }
}
