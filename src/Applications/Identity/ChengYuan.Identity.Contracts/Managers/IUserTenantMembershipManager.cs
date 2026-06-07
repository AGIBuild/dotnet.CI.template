namespace ChengYuan.Identity;

public interface IUserTenantMembershipManager
{
    ValueTask<UserTenantMembershipRecord> AssignAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);

    ValueTask RevokeAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);
}
