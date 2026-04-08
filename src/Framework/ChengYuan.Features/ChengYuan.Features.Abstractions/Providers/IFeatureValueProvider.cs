using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Features;

public interface IFeatureValueProvider
{
    string Name { get; }

    int Order { get; }

    ValueTask<FeatureValue?> GetOrNullAsync(FeatureDefinition definition, FeatureContext context, CancellationToken cancellationToken = default);
}
