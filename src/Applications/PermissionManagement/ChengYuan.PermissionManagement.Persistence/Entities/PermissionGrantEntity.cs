using ChengYuan.Authorization;

namespace ChengYuan.PermissionManagement;

public sealed class PermissionGrantEntity
{
    private string _name = string.Empty;

    private PermissionGrantEntity()
    {
    }

    public PermissionGrantEntity(Guid id, string name, PermissionScope scope, bool isGranted, Guid? tenantId = null, string? userId = null)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        Scope = scope;
        TenantId = tenantId;
        UserId = userId;
        SetName(name);
        IsGranted = isGranted;
    }

    public Guid Id { get; private set; }

    public string Name
    {
        get => _name;
        private set => _name = value;
    }

    public PermissionScope Scope { get; private set; }

    public bool IsGranted { get; private set; }

    public Guid? TenantId { get; private set; }

    public string? UserId { get; private set; }

    public void Update(string name, bool isGranted)
    {
        SetName(name);
        IsGranted = isGranted;
    }

    private void SetName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }
}
