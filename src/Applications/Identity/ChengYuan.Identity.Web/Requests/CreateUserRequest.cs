namespace ChengYuan.Identity;

public sealed class CreateUserRequest
{
    public required string UserName { get; init; }

    public required string Email { get; init; }

    public required string Password { get; init; }
}
