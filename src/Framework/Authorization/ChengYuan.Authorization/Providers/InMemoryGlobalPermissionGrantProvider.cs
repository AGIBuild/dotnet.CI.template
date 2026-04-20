using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Authorization;

internal sealed class InMemoryGlobalPermissionGrantProvider(IReadOnlyDictionary<string, bool> values) : IPermissionGrantProvider
{
    public string Name => "InMemoryGlobal";

    public int Order => 150;

    public ValueTask<PermissionGrantResult> CheckAsync(PermissionDefinition definition, PermissionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (values.TryGetValue(definition.Name, out var isGranted))
        {
            return ValueTask.FromResult(isGranted ? PermissionGrantResult.Granted : PermissionGrantResult.Prohibited);
        }

        return ValueTask.FromResult(PermissionGrantResult.Undefined);
    }
}
