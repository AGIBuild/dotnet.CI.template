namespace ChengYuan.Identity;

public sealed record UserTenantMembershipRecord
{
    public UserTenantMembershipRecord(Guid userId, Guid tenantId, bool isActive)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(userId));
        }

        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant id cannot be empty.", nameof(tenantId));
        }

        UserId = userId;
        TenantId = tenantId;
        IsActive = isActive;
    }

    public Guid UserId { get; }

    public Guid TenantId { get; }

    public bool IsActive { get; }
}
