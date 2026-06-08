using ChengYuan.Core.Data;
using ChengYuan.Core.Guids;

namespace ChengYuan.TenantManagement;

public sealed class TenantManager(
    ITenantStore store,
    IGuidGenerator guidGenerator,
    IUnitOfWork unitOfWork) : ITenantManager
{
    public async ValueTask<TenantRecord> CreateAsync(string name, bool isActive = true, CancellationToken cancellationToken = default)
    {
        var tenant = new TenantRecord(guidGenerator.Create(), name, isActive);
        await store.SetAsync(tenant, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return tenant;
    }

    public async ValueTask SetAsync(TenantRecord tenant, CancellationToken cancellationToken = default)
    {
        await store.SetAsync(tenant, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async ValueTask RemoveAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        await store.RemoveAsync(tenantId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
