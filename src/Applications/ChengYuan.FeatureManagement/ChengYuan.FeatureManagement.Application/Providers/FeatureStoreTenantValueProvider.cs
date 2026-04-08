using ChengYuan.Features;

namespace ChengYuan.FeatureManagement;

internal sealed class FeatureStoreTenantValueProvider(IFeatureValueStore store) : IFeatureValueProvider
{
    public string Name => "FeatureStoreTenant";

    public int Order => 220;

    public async ValueTask<FeatureValue?> GetOrNullAsync(FeatureDefinition definition, FeatureContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (context.TenantId is not Guid tenantId)
        {
            return null;
        }

        var record = await store.FindAsync(definition.Name, FeatureScope.Tenant, tenantId, cancellationToken: cancellationToken);
        return record is null ? null : new FeatureValue(record.Value, Name);
    }
}
