using System.Threading.Tasks;
using ChengYuan.Core.UI.Navigation;

namespace ChengYuan.SettingManagement;

public sealed class SettingManagementMenuContributor : IMenuContributor
{
    public Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        var administration = context.Menu.GetAdministration();

        administration.AddChild(
            new MenuItemDefinition("SettingManagement", "Setting Management", "/settings")
            {
                Icon = "ControlOutlined",
                Order = 400,
                RequiredPermission = SettingManagementPermissions.Settings,
            });

        return Task.CompletedTask;
    }
}
