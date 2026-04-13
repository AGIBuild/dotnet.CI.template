using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.Emailing;

public static class EmailingServiceCollectionExtensions
{
    public static IServiceCollection AddEmailing(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<SmtpOptions>();
        services.TryAddSingleton<IEmailSender, NullEmailSender>();
        return services;
    }

    public static IServiceCollection AddSmtpEmailing(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<SmtpOptions>();
        services.AddSingleton<IEmailSender, SmtpEmailSender>();
        return services;
    }
}
