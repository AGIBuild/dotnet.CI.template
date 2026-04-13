using ChengYuan.Core.Modularity;
using ChengYuan.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.CliHost;

public static class CliHostServiceCollectionExtensions
{
    public static IServiceCollection AddCliHostComposition(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton(new MultiTenancyOptions { IsEnabled = false });
        services.AddModularApplication<CliHostModule>();

        return services;
    }
}
