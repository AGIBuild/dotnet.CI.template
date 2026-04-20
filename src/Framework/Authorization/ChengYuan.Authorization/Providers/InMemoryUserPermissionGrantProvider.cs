using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Authorization;

internal sealed class InMemoryUserPermissionGrantProvider(IReadOnlyDictionary<(string UserId, string Name), bool> values) : IPermissionGrantProvider
{
    public string Name => "InMemoryUser";

    public int Order => 350;

    public ValueTask<PermissionGrantResult> CheckAsync(PermissionDefinition definition, PermissionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (string.IsNullOrWhiteSpace(context.UserId))
        {
            return ValueTask.FromResult(PermissionGrantResult.Undefined);
        }

        if (values.TryGetValue((context.UserId, definition.Name), out var isGranted))
        {
            return ValueTask.FromResult(isGranted ? PermissionGrantResult.Granted : PermissionGrantResult.Prohibited);
        }

        return ValueTask.FromResult(PermissionGrantResult.Undefined);
    }
}
