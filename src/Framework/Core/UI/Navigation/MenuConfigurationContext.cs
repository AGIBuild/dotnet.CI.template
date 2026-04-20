using System;

namespace ChengYuan.Core.UI.Navigation;

public sealed class MenuConfigurationContext
{
    public MenuConfigurationContext(ApplicationMenu menu, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(menu);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        Menu = menu;
        ServiceProvider = serviceProvider;
    }

    public ApplicationMenu Menu { get; }

    public IServiceProvider ServiceProvider { get; }
}
