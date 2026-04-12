using ChengYuan.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.PermissionManagement;

public static class PermissionManagementPersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddPermissionManagementPersistence(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddConfiguredDbContext<PermissionManagementDbContext>();
        services.AddConfiguredDbContextFactory<PermissionManagementDbContext>();
        services.AddEntityFrameworkCoreDataAccess<PermissionManagementDbContext>();
        services.TryAddSingleton<IPermissionGrantStore, EfPermissionGrantStore>();
        services.TryAddSingleton<IPermissionGrantReader>(serviceProvider => serviceProvider.GetRequiredService<IPermissionGrantStore>());

        return services;
    }
}
