using ChengYuan.Core.Data;
using ChengYuan.Core.Entities;

namespace ChengYuan.Identity;

public sealed class IdentityRole : AggregateRoot<Guid>, ISoftDelete
{
    private string _name = string.Empty;
    private string _normalizedName = string.Empty;

    private IdentityRole()
    {
    }

    public IdentityRole(Guid id, string name)
        : base(id)
    {
        Update(name, isActive: true);
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
