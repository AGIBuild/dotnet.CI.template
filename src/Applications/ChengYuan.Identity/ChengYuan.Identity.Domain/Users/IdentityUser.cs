using ChengYuan.Core.Data;
using ChengYuan.Core.Entities;

namespace ChengYuan.Identity;

public sealed class IdentityUser : AggregateRoot<Guid>, ISoftDelete
{
    private string _userName = string.Empty;
    private string _normalizedUserName = string.Empty;
    private string _email = string.Empty;
    private string _normalizedEmail = string.Empty;

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

    public bool IsActive { get; private set; }

    public bool IsDeleted { get; private set; }

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

    public void MarkDeleted()
    {
        IsDeleted = true;
        IsActive = false;
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
