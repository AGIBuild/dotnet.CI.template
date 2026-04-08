using ChengYuan.Core.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.PermissionManagement;

public static class PermissionManagementPersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddPermissionManagementPersistence(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddEntityFrameworkCoreDataAccess<PermissionManagementDbContext>();
        services.TryAddSingleton<IPermissionGrantStore, EfPermissionGrantStore>();
        services.TryAddSingleton<IPermissionGrantReader>(serviceProvider => serviceProvider.GetRequiredService<IPermissionGrantStore>());

        return services;
    }

    public static IServiceCollection AddPermissionManagementPersistenceDbContext(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDbContext)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureDbContext);

        services.AddDbContext<PermissionManagementDbContext>(configureDbContext);
        services.AddDbContextFactory<PermissionManagementDbContext>(configureDbContext);

        return services;
    }
}
