using ChengYuan.Core.Modularity;
using ChengYuan.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.WebHost;

public static class WebHostServiceCollectionExtensions
{
    public static IServiceCollection AddWebHostComposition(
        this IServiceCollection services,
        Action<MultiTenancyBuilder>? configureMultiTenancy = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddMultiTenancy(configureMultiTenancy);
        services.AddWebModularApplication<WebHostModule>();

        return services;
    }
}
