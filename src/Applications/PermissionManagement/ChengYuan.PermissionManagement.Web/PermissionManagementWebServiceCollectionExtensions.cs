using ChengYuan.AspNetCore;
using ChengYuan.Core.UI.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.PermissionManagement;

public static class PermissionManagementWebServiceCollectionExtensions
{
    public static IServiceCollection AddPermissionManagementWeb(this IServiceCollection services)
    {
        services.AddTransient<IEndpointContributor, PermissionManagementEndpointContributor>();
        services.AddTransient<IMenuContributor, PermissionManagementMenuContributor>();
        return services;
    }
}
