using System.Threading.Tasks;
using ChengYuan.Core.UI.Navigation;

namespace ChengYuan.FeatureManagement;

public sealed class FeatureManagementMenuContributor : IMenuContributor
{
    public Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        var administration = context.Menu.GetAdministration();

        administration.AddChild(
            new MenuItemDefinition("FeatureManagement", "Feature Management", "/features")
            {
                Icon = "ExperimentOutlined",
                Order = 500,
                RequiredPermission = FeatureManagementPermissions.Features,
            });

        return Task.CompletedTask;
    }
}
