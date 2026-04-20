using System.Threading.Tasks;
using ChengYuan.Core.UI.Navigation;

namespace ChengYuan.PermissionManagement;

public sealed class PermissionManagementMenuContributor : IMenuContributor
{
    public Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        var administration = context.Menu.GetAdministration();

        administration.AddChild(
            new MenuItemDefinition("PermissionManagement", "Permission Management", "/permissions")
            {
                Icon = "SafetyOutlined",
                Order = 300,
                RequiredPermission = PermissionManagementPermissions.Permissions,
            });

        return Task.CompletedTask;
    }
}
