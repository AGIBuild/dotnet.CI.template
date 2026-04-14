using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.Notifications;

public static class NotificationServiceCollectionExtensions
{
    public static IServiceCollection AddNotifications(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddTransient<INotificationSender, DefaultNotificationSender>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<INotificationChannel, NullNotificationChannel>());

        return services;
    }
}
