using ChengYuan.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.SettingManagement;

public static class SettingManagementPersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddSettingManagementPersistence(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddConfiguredDbContext<SettingManagementDbContext>();
        services.AddConfiguredDbContextFactory<SettingManagementDbContext>();
        services.AddEntityFrameworkCoreDataAccess<SettingManagementDbContext>();
        services.TryAddSingleton<ISettingValueStore, EfSettingValueStore>();
        services.TryAddSingleton<ISettingValueReader>(serviceProvider => serviceProvider.GetRequiredService<ISettingValueStore>());

        return services;
    }
}
