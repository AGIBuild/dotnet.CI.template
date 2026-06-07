namespace ChengYuan.Identity;

public sealed record IdentityAdminSeedOptions(
    bool SeedEnabled,
    string UserName,
    string Email,
    string Password,
    string RoleName)
{
    public static IdentityAdminSeedOptions Disabled { get; } = new(
        SeedEnabled: false,
        UserName: string.Empty,
        Email: string.Empty,
        Password: string.Empty,
        RoleName: string.Empty);
}
