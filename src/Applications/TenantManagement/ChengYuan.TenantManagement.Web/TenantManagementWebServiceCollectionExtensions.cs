using ChengYuan.AspNetCore;
using ChengYuan.Core.UI.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.TenantManagement;

public static class TenantManagementWebServiceCollectionExtensions
{
    public static IServiceCollection AddTenantManagementWeb(this IServiceCollection services)
    {
        services.AddTransient<IEndpointContributor, TenantManagementEndpointContributor>();
        services.AddTransient<IMenuContributor, TenantManagementMenuContributor>();
        return services;
    }
}
