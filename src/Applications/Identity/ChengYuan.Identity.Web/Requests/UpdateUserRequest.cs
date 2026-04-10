namespace ChengYuan.Identity;

public sealed class UpdateUserRequest
{
    public string UserName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public bool IsActive { get; init; }
}
