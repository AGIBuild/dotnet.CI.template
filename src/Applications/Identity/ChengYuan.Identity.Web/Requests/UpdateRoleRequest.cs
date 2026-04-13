namespace ChengYuan.Identity;

public sealed class UpdateRoleRequest
{
    public required string Name { get; init; }

    public required bool IsActive { get; init; }
}
