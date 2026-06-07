namespace ChengYuan.Identity;

public interface IIdentityUserTenantMembershipRepository
{
    ValueTask<IdentityUserTenantMembership?> FindAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);

    ValueTask<IReadOnlyList<IdentityUserTenantMembership>> GetListByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    ValueTask InsertAsync(IdentityUserTenantMembership membership, CancellationToken cancellationToken = default);
}
