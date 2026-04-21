using ChengYuan.Settings;

namespace ChengYuan.AuditLogging;

internal sealed class AuditLoggingSettingDefinitionContributor : ISettingDefinitionContributor
{
    public void Define(ISettingDefinitionContext context)
    {
        var group = context.AddGroup(AuditLoggingSettings.GroupName, "Audit Logging");

        group.AddSetting<int>(AuditLoggingSettings.RetentionDays, 90, "Audit log retention (days)");
    }
}
