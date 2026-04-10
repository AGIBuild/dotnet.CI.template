using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Features;

internal sealed class InMemoryGlobalFeatureValueProvider(IReadOnlyDictionary<string, object?> values) : IFeatureValueProvider
{
    public string Name => "InMemoryGlobal";

    public int Order => 150;

    public ValueTask<FeatureValue?> GetOrNullAsync(FeatureDefinition definition, FeatureContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);

        return ValueTask.FromResult(
            values.TryGetValue(definition.Name, out var value)
                ? new FeatureValue(value, Name)
                : null);
    }
}
