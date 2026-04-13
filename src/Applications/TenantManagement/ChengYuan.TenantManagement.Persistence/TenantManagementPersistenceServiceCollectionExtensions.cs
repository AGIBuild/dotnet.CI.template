using ChengYuan.EntityFrameworkCore;
using ChengYuan.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.TenantManagement;

public static class TenantManagementPersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddTenantManagementPersistence(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddPersistenceModule<TenantManagementDbContext, ITenantStore, ITenantReader, TenantStore>();
        services.Replace(ServiceDescriptor.Singleton<ITenantResolutionStore>(serviceProvider =>
            new TenantResolutionStoreAdapter(serviceProvider.GetRequiredService<ITenantReader>())));

        return services;
    }
}
