using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Authorization;

internal sealed class InMemoryTenantPermissionGrantProvider(IReadOnlyDictionary<(Guid TenantId, string Name), bool> values) : IPermissionGrantProvider
{
    public string Name => "InMemoryTenant";

    public int Order => 250;

    public ValueTask<PermissionGrantResult> CheckAsync(PermissionDefinition definition, PermissionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (context.TenantId is not Guid tenantId)
        {
            return ValueTask.FromResult(PermissionGrantResult.Undefined);
        }

        if (values.TryGetValue((tenantId, definition.Name), out var isGranted))
        {
            return ValueTask.FromResult(isGranted ? PermissionGrantResult.Granted : PermissionGrantResult.Prohibited);
        }

        return ValueTask.FromResult(PermissionGrantResult.Undefined);
    }
}
