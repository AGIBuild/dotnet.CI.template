namespace ChengYuan.Identity;

public sealed class IdentityUserRole
{
    private IdentityUserRole()
    {
    }

    public IdentityUserRole(Guid userId, Guid roleId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(userId));
        }

        if (roleId == Guid.Empty)
        {
            throw new ArgumentException("Role id cannot be empty.", nameof(roleId));
        }

        UserId = userId;
        RoleId = roleId;
    }

    public Guid UserId { get; private set; }

    public Guid RoleId { get; private set; }
}
