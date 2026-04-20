using System.Threading.Tasks;
using ChengYuan.Core.UI.Navigation;

namespace ChengYuan.TenantManagement;

public sealed class TenantManagementMenuContributor : IMenuContributor
{
    public Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        var administration = context.Menu.GetAdministration();

        administration.AddChild(
            new MenuItemDefinition("TenantManagement", "Tenant Management", "/tenants")
            {
                Icon = "TeamOutlined",
                Order = 100,
                RequiredPermission = TenantManagementPermissions.Tenants,
            });

        return Task.CompletedTask;
    }
}
