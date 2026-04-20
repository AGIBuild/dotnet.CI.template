using ChengYuan.Authorization;

namespace ChengYuan.AuditLogging;

internal sealed class AuditLoggingPermissionDefinitionContributor : IPermissionDefinitionContributor
{
    public void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(AuditLoggingPermissions.GroupName, "Audit Logging");
        group.AddPermission(AuditLoggingPermissions.AuditLogs, "Audit Logs");
    }
}
