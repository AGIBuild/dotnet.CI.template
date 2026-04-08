using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Features;

internal sealed class TenantFeatureValueProvider : IFeatureValueProvider
{
    public string Name => "Tenant";

    public int Order => 200;

    public ValueTask<FeatureValue?> GetOrNullAsync(FeatureDefinition definition, FeatureContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return ValueTask.FromResult<FeatureValue?>(null);
    }
}
