using System.Threading.Tasks;
using ChengYuan.Core.UI.Navigation;

namespace ChengYuan.Identity;

public sealed class IdentityMenuContributor : IMenuContributor
{
    public Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        var administration = context.Menu.GetAdministration();

        var identity = new MenuItemDefinition("Identity", "Identity Management")
        {
            Icon = "UserOutlined",
            Order = 200,
        };

        identity.AddChild(new MenuItemDefinition("Identity.Users", "User Management", "/identity/users")
        {
            Icon = "UserOutlined",
            Order = 100,
            RequiredPermission = IdentityPermissions.Users,
        });

        identity.AddChild(new MenuItemDefinition("Identity.Roles", "Role Management", "/identity/roles")
        {
            Icon = "TeamOutlined",
            Order = 200,
            RequiredPermission = IdentityPermissions.Roles,
        });

        administration.AddChild(identity);

        return Task.CompletedTask;
    }
}
