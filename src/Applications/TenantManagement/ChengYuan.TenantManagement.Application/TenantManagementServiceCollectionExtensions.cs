using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.TenantManagement;

public static class TenantManagementServiceCollectionExtensions
{
    public static IServiceCollection AddTenantManagement(this IServiceCollection services)
    {
        services.TryAddScoped<ITenantManager, TenantManager>();
        return services;
    }

    public static IServiceCollection AddInMemoryTenantManagement(this IServiceCollection services)
    {
        services.TryAddSingleton<InMemoryTenantStore>();
        services.TryAddSingleton<ITenantStore>(serviceProvider => serviceProvider.GetRequiredService<InMemoryTenantStore>());
        services.TryAddSingleton<ITenantReader>(serviceProvider => serviceProvider.GetRequiredService<InMemoryTenantStore>());

        return services;
    }
}
