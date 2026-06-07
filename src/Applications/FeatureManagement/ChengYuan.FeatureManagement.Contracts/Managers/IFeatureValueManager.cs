using ChengYuan.Features;

namespace ChengYuan.FeatureManagement;

public interface IFeatureValueManager
{
    ValueTask<IReadOnlyList<FeatureValueRecord>> GetListAsync(CancellationToken cancellationToken = default);

    ValueTask SetAsync(FeatureValueRecord record, CancellationToken cancellationToken = default);

    ValueTask RemoveAsync(
        string name,
        FeatureScope scope,
        Guid? tenantId = null,
        string? userId = null,
        CancellationToken cancellationToken = default);
}
