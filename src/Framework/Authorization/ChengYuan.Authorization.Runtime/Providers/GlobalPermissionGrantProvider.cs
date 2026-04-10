using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Authorization;

internal sealed class GlobalPermissionGrantProvider : IPermissionGrantProvider
{
    public string Name => "Global";

    public int Order => 100;

    public ValueTask<PermissionGrant?> GetOrNullAsync(PermissionDefinition definition, PermissionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return ValueTask.FromResult<PermissionGrant?>(null);
    }
}
