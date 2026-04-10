using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Features;

internal sealed class GlobalFeatureValueProvider : IFeatureValueProvider
{
    public string Name => "Global";

    public int Order => 100;

    public ValueTask<FeatureValue?> GetOrNullAsync(FeatureDefinition definition, FeatureContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return ValueTask.FromResult<FeatureValue?>(null);
    }
}
