namespace ChengYuan.Identity;

public sealed class UpdateUserRequest
{
    public required string UserName { get; init; }

    public required string Email { get; init; }

    public required bool IsActive { get; init; }
}
