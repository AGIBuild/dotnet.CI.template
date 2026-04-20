using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChengYuan.Core.UI.Navigation;

public sealed class DefaultMenuManager(IEnumerable<IMenuContributor> contributors, IServiceProvider serviceProvider)
    : IMenuManager
{
    public async Task<ApplicationMenu> GetAsync(string menuName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(menuName);

        var menu = new ApplicationMenu(menuName);
        var context = new MenuConfigurationContext(menu, serviceProvider);

        foreach (var contributor in contributors)
        {
            await contributor.ConfigureMenuAsync(context);
        }

        return menu;
    }
}
