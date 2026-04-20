using System.Threading.Tasks;
using ChengYuan.Core.UI.Navigation;

namespace ChengYuan.AuditLogging;

public sealed class AuditLoggingMenuContributor : IMenuContributor
{
    public Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        var administration = context.Menu.GetAdministration();

        administration.AddChild(
            new MenuItemDefinition("AuditLogging", "Audit Logs", "/audit-logs")
            {
                Icon = "AuditOutlined",
                Order = 600,
                RequiredPermission = AuditLoggingPermissions.AuditLogs,
            });

        return Task.CompletedTask;
    }
}
