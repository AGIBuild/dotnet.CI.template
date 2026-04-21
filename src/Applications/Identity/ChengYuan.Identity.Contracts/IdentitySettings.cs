namespace ChengYuan.Identity;

public static class IdentitySettings
{
    public const string GroupName = "Identity";

    public const string PasswordMinLength = GroupName + ".Password.MinLength";
    public const string PasswordRequireDigit = GroupName + ".Password.RequireDigit";
    public const string PasswordRequireUppercase = GroupName + ".Password.RequireUppercase";
    public const string LockoutMaxAttempts = GroupName + ".Lockout.MaxAttempts";
    public const string LockoutDurationMinutes = GroupName + ".Lockout.DurationMinutes";
}
