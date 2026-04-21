using ChengYuan.Settings;

namespace ChengYuan.Identity;

internal sealed class IdentitySettingDefinitionContributor : ISettingDefinitionContributor
{
    public void Define(ISettingDefinitionContext context)
    {
        var group = context.AddGroup(IdentitySettings.GroupName, "Identity");

        group.AddSetting<int>(IdentitySettings.PasswordMinLength, 6, "Password minimum length");
        group.AddSetting<bool>(IdentitySettings.PasswordRequireDigit, true, "Password requires digit");
        group.AddSetting<bool>(IdentitySettings.PasswordRequireUppercase, true, "Password requires uppercase");
        group.AddSetting<int>(IdentitySettings.LockoutMaxAttempts, 5, "Lockout max failed attempts");
        group.AddSetting<int>(IdentitySettings.LockoutDurationMinutes, 15, "Lockout duration (minutes)");
    }
}
