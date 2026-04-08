namespace ChengYuan.Identity;

public sealed class UserRecord
{
    public UserRecord(Guid id, string userName, string email, bool isActive)
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
    }

    public Guid Id { get; }

    public string UserName { get; }

    public string Email { get; }

    public bool IsActive { get; }
}
