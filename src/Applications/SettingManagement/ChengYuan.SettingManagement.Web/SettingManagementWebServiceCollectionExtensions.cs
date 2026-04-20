using ChengYuan.AspNetCore;
using ChengYuan.Core.UI.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.SettingManagement;

public static class SettingManagementWebServiceCollectionExtensions
{
    public static IServiceCollection AddSettingManagementWeb(this IServiceCollection services)
    {
        services.AddTransient<IEndpointContributor, SettingManagementEndpointContributor>();
        services.AddTransient<IMenuContributor, SettingManagementMenuContributor>();
        return services;
    }
}
