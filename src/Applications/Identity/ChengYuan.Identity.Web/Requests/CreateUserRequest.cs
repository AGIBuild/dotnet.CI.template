namespace ChengYuan.Identity;

public sealed class CreateUserRequest
{
    public string UserName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;
}
