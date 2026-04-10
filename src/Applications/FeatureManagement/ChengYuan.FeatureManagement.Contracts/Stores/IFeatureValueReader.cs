using ChengYuan.Features;

namespace ChengYuan.FeatureManagement;

public interface IFeatureValueReader
{
    ValueTask<FeatureValueRecord?> FindAsync(
        string name,
        FeatureScope scope,
        Guid? tenantId = null,
        string? userId = null,
        CancellationToken cancellationToken = default);

    ValueTask<IReadOnlyList<FeatureValueRecord>> GetListAsync(CancellationToken cancellationToken = default);
}
