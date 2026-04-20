using ChengYuan.AspNetCore;
using ChengYuan.Core.UI.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.FeatureManagement;

public static class FeatureManagementWebServiceCollectionExtensions
{
    public static IServiceCollection AddFeatureManagementWeb(this IServiceCollection services)
    {
        services.AddTransient<IEndpointContributor, FeatureManagementEndpointContributor>();
        services.AddTransient<IMenuContributor, FeatureManagementMenuContributor>();
        return services;
    }
}
