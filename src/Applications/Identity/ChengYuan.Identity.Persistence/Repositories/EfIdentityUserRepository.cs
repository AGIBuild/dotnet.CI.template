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
    private IQueryable<IdentityUser> UserQuery => DbContext.Users
        .Where(user => !user.IsDeleted)
        .Include(user => user.Roles);

    public async ValueTask<IdentityUser?> FindDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(id));
        }

        return await UserQuery.SingleOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public async ValueTask<IdentityUser> GetDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await FindDetailsAsync(id, cancellationToken);
        return user ?? throw new InvalidOperationException($"Identity user '{id}' was not found.");
    }

    public async ValueTask<IdentityUser?> FindByNormalizedUserNameAsync(string normalizedUserName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedUserName);

        return await UserQuery.SingleOrDefaultAsync(user => user.NormalizedUserName == normalizedUserName, cancellationToken);
    }

    public async ValueTask<IdentityUser?> FindByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedEmail);

        return await UserQuery.SingleOrDefaultAsync(user => user.NormalizedEmail == normalizedEmail, cancellationToken);
    }

    public async ValueTask<IReadOnlyList<IdentityUser>> GetListByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        if (roleId == Guid.Empty)
        {
            throw new ArgumentException("Role id cannot be empty.", nameof(roleId));
        }

        return await UserQuery
            .Where(user => user.Roles.Any(userRole => userRole.RoleId == roleId))
            .OrderBy(user => user.NormalizedUserName)
            .ThenBy(user => user.Id)
            .ToArrayAsync(cancellationToken);
    }

    public async ValueTask<IReadOnlyList<IdentityUser>> GetListAsync(CancellationToken cancellationToken = default)
    {
        return await UserQuery
            .OrderBy(user => user.NormalizedUserName)
            .ThenBy(user => user.Id)
            .ToArrayAsync(cancellationToken);
    }
}
