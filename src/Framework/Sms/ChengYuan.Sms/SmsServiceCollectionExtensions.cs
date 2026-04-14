using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.Sms;

public static class SmsServiceCollectionExtensions
{
    public static IServiceCollection AddSms(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<ISmsSender, NullSmsSender>();
        return services;
    }
}
