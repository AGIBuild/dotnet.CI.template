using ChengYuan.Core.Data;
using ChengYuan.Core.Timing;
using ChengYuan.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChengYuan.Identity;

public sealed class EfIdentityRoleRepository(
    IdentityDbContext dbContext,
    IDataFilter<SoftDeleteFilter>? softDeleteFilter = null,
    IDataFilter<MultiTenantFilter>? multiTenantFilter = null,
    IDataTenantProvider? dataTenantProvider = null,
    IClock? clock = null)
    : EfRepository<IdentityDbContext, IdentityRole, Guid>(dbContext, softDeleteFilter, multiTenantFilter, dataTenantProvider, clock), IIdentityRoleRepository
{
    public async ValueTask<IdentityRole?> FindByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedName);

        return await Query.SingleOrDefaultAsync(role => role.NormalizedName == normalizedName, cancellationToken);
    }

    public async ValueTask<IReadOnlyList<IdentityRole>> GetListAsync(CancellationToken cancellationToken = default)
    {
        return await Query
            .OrderBy(role => role.NormalizedName)
            .ThenBy(role => role.Id)
            .ToArrayAsync(cancellationToken);
    }
}
