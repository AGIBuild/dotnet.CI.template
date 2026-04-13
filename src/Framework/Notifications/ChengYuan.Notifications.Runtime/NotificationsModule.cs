using ChengYuan.Core.Modularity;

namespace ChengYuan.Notifications;

public sealed class NotificationsModule : FrameworkCoreModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddNotifications();
    }
}
