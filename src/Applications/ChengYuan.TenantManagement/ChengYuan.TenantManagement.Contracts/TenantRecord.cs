namespace ChengYuan.TenantManagement;

public sealed class TenantRecord
{
    public TenantRecord(Guid id, string name, bool isActive = true)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Tenant id cannot be empty.", nameof(id));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Id = id;
        Name = name;
        IsActive = isActive;
    }

    public Guid Id { get; }

    public string Name { get; }

    public bool IsActive { get; }
}
