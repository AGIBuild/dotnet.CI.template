using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Features;

internal sealed class InMemoryTenantFeatureValueProvider(IReadOnlyDictionary<(Guid TenantId, string Name), object?> values) : IFeatureValueProvider
{
    public string Name => "InMemoryTenant";

    public int Order => 250;

    public ValueTask<FeatureValue?> GetOrNullAsync(FeatureDefinition definition, FeatureContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (context.TenantId is not Guid tenantId)
        {
            return ValueTask.FromResult<FeatureValue?>(null);
        }

        return ValueTask.FromResult(
            values.TryGetValue((tenantId, definition.Name), out var value)
                ? new FeatureValue(value, Name)
                : null);
    }
}
