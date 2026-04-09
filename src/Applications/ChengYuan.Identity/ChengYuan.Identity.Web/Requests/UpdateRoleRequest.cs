namespace ChengYuan.Identity;

public sealed class UpdateRoleRequest
{
    public string Name { get; init; } = string.Empty;

    public bool IsActive { get; init; }
}
