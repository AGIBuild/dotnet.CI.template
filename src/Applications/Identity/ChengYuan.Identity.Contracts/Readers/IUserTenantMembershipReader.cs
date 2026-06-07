namespace ChengYuan.Identity;

public interface IUserTenantMembershipReader
{
    ValueTask<UserTenantMembershipRecord?> FindAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);

    ValueTask<bool> IsActiveMemberAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);

    ValueTask<IReadOnlyList<UserTenantMembershipRecord>> GetListByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
