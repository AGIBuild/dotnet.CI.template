namespace ChengYuan.AspNetCore.Configuration;

public sealed class CurrentUserDto
{
    public string? Id { get; init; }

    public string? UserName { get; init; }

    public bool IsAuthenticated { get; init; }
}
