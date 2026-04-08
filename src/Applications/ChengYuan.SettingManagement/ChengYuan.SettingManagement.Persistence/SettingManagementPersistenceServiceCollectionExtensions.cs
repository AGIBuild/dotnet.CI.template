using ChengYuan.Core.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.SettingManagement;

public static class SettingManagementPersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddSettingManagementPersistence(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddEntityFrameworkCoreDataAccess<SettingManagementDbContext>();
        services.TryAddSingleton<ISettingValueStore, EfSettingValueStore>();
        services.TryAddSingleton<ISettingValueReader>(serviceProvider => serviceProvider.GetRequiredService<ISettingValueStore>());

        return services;
    }

    public static IServiceCollection AddSettingManagementPersistenceDbContext(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDbContext)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureDbContext);

        services.AddDbContext<SettingManagementDbContext>(configureDbContext);
        services.AddDbContextFactory<SettingManagementDbContext>(configureDbContext);

        return services;
    }
}
