using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Authorization;

internal sealed class InMemoryGlobalPermissionGrantProvider(IReadOnlyDictionary<string, bool> values) : IPermissionGrantProvider
{
    public string Name => "InMemoryGlobal";

    public int Order => 150;

    public ValueTask<PermissionGrant?> GetOrNullAsync(PermissionDefinition definition, PermissionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);

        return ValueTask.FromResult(
            values.TryGetValue(definition.Name, out var isGranted)
                ? new PermissionGrant(isGranted, Name)
                : null);
    }
}
