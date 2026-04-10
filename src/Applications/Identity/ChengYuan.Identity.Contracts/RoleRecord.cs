namespace ChengYuan.Identity;

public sealed class RoleRecord
{
    public RoleRecord(Guid id, string name, bool isActive)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Role id cannot be empty.", nameof(id));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Id = id;
        Name = name.Trim();
        IsActive = isActive;
    }

    public Guid Id { get; }

    public string Name { get; }

    public bool IsActive { get; }
}
