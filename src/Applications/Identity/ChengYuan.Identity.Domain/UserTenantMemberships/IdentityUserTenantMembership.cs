namespace ChengYuan.Identity;

public sealed class IdentityUserTenantMembership
{
    private IdentityUserTenantMembership()
    {
    }

    public IdentityUserTenantMembership(Guid userId, Guid tenantId)
    {
        EnsureId(userId, nameof(userId), "User");
        EnsureId(tenantId, nameof(tenantId), "Tenant");

        UserId = userId;
        TenantId = tenantId;
        IsActive = true;
    }

    public Guid UserId { get; private set; }

    public Guid TenantId { get; private set; }

    public bool IsActive { get; private set; }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    private static void EnsureId(Guid id, string parameterName, string label)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException($"{label} id cannot be empty.", parameterName);
        }
    }
}
