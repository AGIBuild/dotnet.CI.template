using ChengYuan.Features;

namespace ChengYuan.FeatureManagement;

public sealed class FeatureValueManager(IFeatureValueStore store) : IFeatureValueManager
{
    public ValueTask SetAsync(FeatureValueRecord record, CancellationToken cancellationToken = default)
    {
        return store.SetAsync(record, cancellationToken);
    }

    public ValueTask RemoveAsync(
        string name,
        FeatureScope scope,
        Guid? tenantId = null,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        return store.RemoveAsync(name, scope, tenantId, userId, cancellationToken);
    }
}
