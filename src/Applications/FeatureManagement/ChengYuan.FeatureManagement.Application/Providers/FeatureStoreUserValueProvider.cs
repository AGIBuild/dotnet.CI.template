using ChengYuan.Features;

namespace ChengYuan.FeatureManagement;

internal sealed class FeatureStoreUserValueProvider(IFeatureValueStore store) : IFeatureValueProvider
{
    public string Name => "FeatureStoreUser";

    public int Order => 320;

    public async ValueTask<FeatureValue?> GetOrNullAsync(FeatureDefinition definition, FeatureContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (string.IsNullOrWhiteSpace(context.UserId))
        {
            return null;
        }

        var record = await store.FindAsync(definition.Name, FeatureScope.User, userId: context.UserId, cancellationToken: cancellationToken);
        return record is null ? null : new FeatureValue(record.Value, Name);
    }
}
