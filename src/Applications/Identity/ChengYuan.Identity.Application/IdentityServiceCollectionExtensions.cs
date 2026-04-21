using ChengYuan.Authorization;
using ChengYuan.Core.DependencyInjection;
using ChengYuan.Features;
using ChengYuan.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Identity;

public static class IdentityServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddConventionalServices(typeof(IdentityServiceCollectionExtensions).Assembly);
        services.AddTransient<IPermissionDefinitionContributor, IdentityPermissionDefinitionContributor>();
        services.AddTransient<ISettingDefinitionContributor, IdentitySettingDefinitionContributor>();
        services.AddTransient<IFeatureDefinitionContributor, IdentityFeatureDefinitionContributor>();

        return services;
    }
}
