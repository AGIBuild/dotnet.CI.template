using ChengYuan.Core.Data;
using ChengYuan.Core.EntityFrameworkCore;
using ChengYuan.Core.Timing;
using Microsoft.EntityFrameworkCore;

namespace ChengYuan.Identity;

public sealed class EfIdentityUserRepository(
    IdentityDbContext dbContext,
    IDataFilter<SoftDeleteFilter>? softDeleteFilter = null,
    IDataFilter<MultiTenantFilter>? multiTenantFilter = null,
    IDataTenantProvider? dataTenantProvider = null,
    IClock? clock = null)
    : EfRepository<IdentityDbContext, IdentityUser, Guid>(dbContext, softDeleteFilter, multiTenantFilter, dataTenantProvider, clock), IIdentityUserRepository
{
    public async ValueTask<IdentityUser?> FindByNormalizedUserNameAsync(string normalizedUserName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedUserName);

        return await Query.SingleOrDefaultAsync(user => user.NormalizedUserName == normalizedUserName, cancellationToken);
    }

    public async ValueTask<IdentityUser?> FindByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedEmail);

        return await Query.SingleOrDefaultAsync(user => user.NormalizedEmail == normalizedEmail, cancellationToken);
    }

    public async ValueTask<IReadOnlyList<IdentityUser>> GetListAsync(CancellationToken cancellationToken = default)
    {
        return await Query
            .OrderBy(user => user.NormalizedUserName)
            .ThenBy(user => user.Id)
            .ToArrayAsync(cancellationToken);
    }
}
