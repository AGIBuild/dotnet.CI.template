using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.TextTemplating;

public static class TextTemplatingServiceCollectionExtensions
{
    public static IServiceCollection AddTextTemplating(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<TextTemplatingOptions>();
        services.TryAddSingleton<ITemplateRenderer, DefaultTemplateRenderer>();
        return services;
    }
}
