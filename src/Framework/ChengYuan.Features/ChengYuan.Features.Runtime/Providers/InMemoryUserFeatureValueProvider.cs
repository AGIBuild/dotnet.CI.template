using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Features;

internal sealed class InMemoryUserFeatureValueProvider(IReadOnlyDictionary<(string UserId, string Name), object?> values) : IFeatureValueProvider
{
    public string Name => "InMemoryUser";

    public int Order => 350;

    public ValueTask<FeatureValue?> GetOrNullAsync(FeatureDefinition definition, FeatureContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (string.IsNullOrWhiteSpace(context.UserId))
        {
            return ValueTask.FromResult<FeatureValue?>(null);
        }

        return ValueTask.FromResult(
            values.TryGetValue((context.UserId, definition.Name), out var value)
                ? new FeatureValue(value, Name)
                : null);
    }
}
