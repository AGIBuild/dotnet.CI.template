using System.Linq;

namespace ChengYuan.Identity;

public sealed record UserRecord
{
    public UserRecord(Guid id, string userName, string email, bool isActive, IReadOnlyList<Guid>? roleIds = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(id));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(userName);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        Id = id;
        UserName = userName.Trim();
        Email = email.Trim();
        IsActive = isActive;
        RoleIds = (roleIds ?? Array.Empty<Guid>())
            .Where(static roleId => roleId != Guid.Empty)
            .Distinct()
            .OrderBy(static roleId => roleId)
            .ToArray();
    }

    public Guid Id { get; }

    public string UserName { get; }

    public string Email { get; }

    public bool IsActive { get; }

    public IReadOnlyList<Guid> RoleIds { get; }
}
