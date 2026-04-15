using System.Linq;
using ChengYuan.Core.Data;
using ChengYuan.Core.Entities;

namespace ChengYuan.Identity;

public sealed class IdentityUser : AggregateRoot<Guid>, ISoftDelete
{
    private readonly List<IdentityUserRole> _roles = [];
    private string _userName = string.Empty;
    private string _normalizedUserName = string.Empty;
    private string _email = string.Empty;
    private string _normalizedEmail = string.Empty;
    private string? _passwordHash;

    private IdentityUser()
    {
    }

    public IdentityUser(Guid id, string userName, string email)
        : base(id)
    {
        Update(userName, email, isActive: true);
    }

    public string UserName
    {
        get => _userName;
        private set => _userName = value;
    }

    public string NormalizedUserName
    {
        get => _normalizedUserName;
        private set => _normalizedUserName = value;
    }

    public string Email
    {
        get => _email;
        private set => _email = value;
    }

    public string NormalizedEmail
    {
        get => _normalizedEmail;
        private set => _normalizedEmail = value;
    }

    public string? PasswordHash
    {
        get => _passwordHash;
        private set => _passwordHash = value;
    }

    public bool IsActive { get; private set; }

    public bool IsDeleted { get; private set; }

    public IReadOnlyCollection<IdentityUserRole> Roles => _roles.AsReadOnly();

    public void Update(string userName, string email, bool isActive)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userName);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        UserName = userName.Trim();
        NormalizedUserName = NormalizeUserName(userName);
        Email = email.Trim();
        NormalizedEmail = NormalizeEmail(email);
        IsActive = isActive;
    }

    public void SetPasswordHash(string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        PasswordHash = passwordHash;
    }

    public void MarkDeleted()
    {
        IsDeleted = true;
        IsActive = false;
    }

    public void AssignRole(Guid roleId)
    {
        if (roleId == Guid.Empty)
        {
            throw new ArgumentException("Role id cannot be empty.", nameof(roleId));
        }

        if (_roles.Any(userRole => userRole.RoleId == roleId))
        {
            return;
        }

        _roles.Add(new IdentityUserRole(Id, roleId));
    }

    public void UnassignRole(Guid roleId)
    {
        if (roleId == Guid.Empty)
        {
            throw new ArgumentException("Role id cannot be empty.", nameof(roleId));
        }

        var existingRole = _roles.FirstOrDefault(userRole => userRole.RoleId == roleId);
        if (existingRole is null)
        {
            return;
        }

        _roles.Remove(existingRole);
    }

    public static string NormalizeUserName(string userName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userName);
        return userName.Trim().ToUpperInvariant();
    }

    public static string NormalizeEmail(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        return email.Trim().ToUpperInvariant();
    }
}
