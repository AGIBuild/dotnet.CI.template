using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Features;

internal sealed class UserFeatureValueProvider : IFeatureValueProvider
{
    public string Name => "User";

    public int Order => 300;

    public ValueTask<FeatureValue?> GetOrNullAsync(FeatureDefinition definition, FeatureContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return ValueTask.FromResult<FeatureValue?>(null);
    }
}
