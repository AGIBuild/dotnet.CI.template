using ChengYuan.Core.Data;
using ChengYuan.ExecutionContext;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.MultiTenancy;

public static class MultiTenancyServiceCollectionExtensions
{
    public static IServiceCollection AddMultiTenancy(this IServiceCollection services)
    {
        services.AddSingleton<ICurrentTenantAccessor, CurrentTenantAccessor>();
        services.AddSingleton<ICurrentTenant>(serviceProvider => serviceProvider.GetRequiredService<ICurrentTenantAccessor>());
        services.Replace(ServiceDescriptor.Singleton<IDataTenantProvider>(
            serviceProvider => new CurrentTenantDataTenantProvider(serviceProvider.GetRequiredService<ICurrentTenant>())));
        return services;
    }
}
