using ChengYuan.Identity;

namespace ChengYuan.FrameworkKernel.Tests;

internal sealed class InMemoryIdentityUserTenantMembershipRepository : IIdentityUserTenantMembershipRepository
{
    private readonly object _sync = new();
    private readonly Dictionary<(Guid UserId, Guid TenantId), IdentityUserTenantMembership> _memberships = [];

    public ValueTask<IdentityUserTenantMembership?> FindAsync(
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        EnsureId(userId, nameof(userId), "User");
        EnsureId(tenantId, nameof(tenantId), "Tenant");

        lock (_sync)
        {
            return ValueTask.FromResult(_memberships.GetValueOrDefault((userId, tenantId)));
        }
    }

    public ValueTask<IReadOnlyList<IdentityUserTenantMembership>> GetListByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        EnsureId(userId, nameof(userId), "User");

        lock (_sync)
        {
            var memberships = _memberships.Values
                .Where(membership => membership.UserId == userId)
                .OrderBy(membership => membership.TenantId)
                .ToArray();

            return ValueTask.FromResult<IReadOnlyList<IdentityUserTenantMembership>>(memberships);
        }
    }

    public ValueTask InsertAsync(IdentityUserTenantMembership membership, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(membership);

        lock (_sync)
        {
            _memberships.Add((membership.UserId, membership.TenantId), membership);
        }

        return ValueTask.CompletedTask;
    }

    private static void EnsureId(Guid id, string parameterName, string label)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException($"{label} id cannot be empty.", parameterName);
        }
    }
}
