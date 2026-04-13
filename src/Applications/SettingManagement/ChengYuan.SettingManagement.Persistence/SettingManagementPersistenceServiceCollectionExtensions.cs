using ChengYuan.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.SettingManagement;

public static class SettingManagementPersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddSettingManagementPersistence(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddPersistenceModule<SettingManagementDbContext, ISettingValueStore, ISettingValueReader, SettingValueStore>();

        return services;
    }
}
