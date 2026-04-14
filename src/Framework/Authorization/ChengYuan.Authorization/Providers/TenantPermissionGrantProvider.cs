using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Authorization;

internal sealed class TenantPermissionGrantProvider : IPermissionGrantProvider
{
    public string Name => "Tenant";

    public int Order => 200;

    public ValueTask<PermissionGrant?> GetOrNullAsync(PermissionDefinition definition, PermissionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return ValueTask.FromResult<PermissionGrant?>(null);
    }
}
