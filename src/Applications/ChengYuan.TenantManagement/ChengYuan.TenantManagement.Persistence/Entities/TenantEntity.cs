using ChengYuan.Core.Data;
using ChengYuan.Core.Entities;

namespace ChengYuan.TenantManagement;

public sealed class TenantEntity : AggregateRoot<Guid>, ISoftDelete
{
    private string _name = string.Empty;
    private string _normalizedName = string.Empty;

    private TenantEntity()
    {
    }

    public TenantEntity(Guid id, string name, bool isActive)
        : base(id)
    {
        Update(name, isActive);
    }

    public string Name
    {
        get => _name;
        private set => _name = value;
    }

    public string NormalizedName
    {
        get => _normalizedName;
        private set => _normalizedName = value;
    }

    public bool IsActive { get; private set; }

    public bool IsDeleted { get; private set; }

    public void Update(string name, bool isActive)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name.Trim();
        NormalizedName = NormalizeName(name);
        IsActive = isActive;
    }

    public void MarkDeleted()
    {
        IsDeleted = true;
        IsActive = false;
    }

    public static string NormalizeName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return name.Trim().ToUpperInvariant();
    }
}
