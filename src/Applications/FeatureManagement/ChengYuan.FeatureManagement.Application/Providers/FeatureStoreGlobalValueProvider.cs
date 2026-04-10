using ChengYuan.Features;

namespace ChengYuan.FeatureManagement;

internal sealed class FeatureStoreGlobalValueProvider(IFeatureValueStore store) : IFeatureValueProvider
{
    public string Name => "FeatureStoreGlobal";

    public int Order => 120;

    public async ValueTask<FeatureValue?> GetOrNullAsync(FeatureDefinition definition, FeatureContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var record = await store.FindAsync(definition.Name, FeatureScope.Global, cancellationToken: cancellationToken);
        return record is null ? null : new FeatureValue(record.Value, Name);
    }
}
