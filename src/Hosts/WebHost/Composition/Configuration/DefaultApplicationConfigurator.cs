using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChengYuan.AspNetCore.Configuration;
using ChengYuan.Authorization;
using ChengYuan.Core.UI.Navigation;
using ChengYuan.ExecutionContext;

namespace ChengYuan.WebHost;

internal sealed class DefaultApplicationConfigurator(
    IMenuManager menuManager,
    IPermissionDefinitionManager permissionDefinitionManager,
    IPermissionChecker permissionChecker,
    ICurrentUser currentUser) : IApplicationConfigurator
{
    public async Task<ApplicationConfigurationDto> BuildAsync(CancellationToken cancellationToken = default)
    {
        var menu = await menuManager.GetAsync(StandardMenus.Main);

        var granted = new List<string>();
        foreach (var definition in permissionDefinitionManager.GetAll())
        {
            if (await permissionChecker.IsGrantedAsync(definition.Name, cancellationToken))
            {
                granted.Add(definition.Name);
            }
        }

        return new ApplicationConfigurationDto
        {
            CurrentUser = new CurrentUserDto
            {
                Id = currentUser.Id,
                UserName = currentUser.UserName,
                IsAuthenticated = currentUser.IsAuthenticated,
            },
            Menu = menu,
            GrantedPermissions = granted,
        };
    }
}
