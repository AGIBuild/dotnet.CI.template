using ChengYuan.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.PermissionManagement;

public static class PermissionManagementPersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddPermissionManagementPersistence(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddPersistenceModule<PermissionManagementDbContext, IPermissionGrantStore, IPermissionGrantReader, EfPermissionGrantStore>();

        return services;
    }
}
