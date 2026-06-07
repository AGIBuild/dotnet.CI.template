using Microsoft.EntityFrameworkCore;

namespace ChengYuan.Identity;

public sealed class IdentityUserTenantMembershipRepository(IdentityDbContext dbContext) : IIdentityUserTenantMembershipRepository
{
    public async ValueTask<IdentityUserTenantMembership?> FindAsync(
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        EnsureId(userId, nameof(userId), "User");
        EnsureId(tenantId, nameof(tenantId), "Tenant");

        return await dbContext.UserTenantMemberships
            .SingleOrDefaultAsync(
                membership => membership.UserId == userId && membership.TenantId == tenantId,
                cancellationToken);
    }

    public async ValueTask<IReadOnlyList<IdentityUserTenantMembership>> GetListByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        EnsureId(userId, nameof(userId), "User");

        return await dbContext.UserTenantMemberships
            .Where(membership => membership.UserId == userId)
            .OrderBy(membership => membership.TenantId)
            .ToArrayAsync(cancellationToken);
    }

    public async ValueTask InsertAsync(
        IdentityUserTenantMembership membership,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(membership);

        await dbContext.UserTenantMemberships.AddAsync(membership, cancellationToken);
    }

    private static void EnsureId(Guid id, string parameterName, string label)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException($"{label} id cannot be empty.", parameterName);
        }
    }
}
