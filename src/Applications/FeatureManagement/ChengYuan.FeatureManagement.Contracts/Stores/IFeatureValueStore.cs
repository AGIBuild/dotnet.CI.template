using ChengYuan.Features;

namespace ChengYuan.FeatureManagement;

public interface IFeatureValueStore : IFeatureValueReader
{
    ValueTask SetAsync(FeatureValueRecord record, CancellationToken cancellationToken = default);

    ValueTask RemoveAsync(
        string name,
        FeatureScope scope,
        Guid? tenantId = null,
        string? userId = null,
        CancellationToken cancellationToken = default);
}
