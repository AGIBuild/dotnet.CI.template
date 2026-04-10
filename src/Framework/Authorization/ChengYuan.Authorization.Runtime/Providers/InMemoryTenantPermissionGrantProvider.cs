using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Authorization;

internal sealed class InMemoryTenantPermissionGrantProvider(IReadOnlyDictionary<(Guid TenantId, string Name), bool> values) : IPermissionGrantProvider
{
    public string Name => "InMemoryTenant";

    public int Order => 250;

    public ValueTask<PermissionGrant?> GetOrNullAsync(PermissionDefinition definition, PermissionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (context.TenantId is not Guid tenantId)
        {
            return ValueTask.FromResult<PermissionGrant?>(null);
        }

        return ValueTask.FromResult(
            values.TryGetValue((tenantId, definition.Name), out var isGranted)
                ? new PermissionGrant(isGranted, Name)
                : null);
    }
}
