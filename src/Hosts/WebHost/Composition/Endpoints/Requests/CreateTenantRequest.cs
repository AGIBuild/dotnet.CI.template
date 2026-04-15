namespace ChengYuan.WebHost;

public sealed class CreateTenantRequest
{
    public required string Name { get; init; }

    public bool IsActive { get; init; } = true;
}
