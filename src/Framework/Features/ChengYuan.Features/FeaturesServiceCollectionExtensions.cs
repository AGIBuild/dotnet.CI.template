using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.Features;

public static class FeaturesServiceCollectionExtensions
{
    public static IServiceCollection AddFeatures(this IServiceCollection services)
    {
        services.TryAddSingleton<IFeatureDefinitionManager, DefaultFeatureDefinitionManager>();
        services.TryAddSingleton<IFeatureChecker, DefaultFeatureChecker>();

        return services;
    }

    public static IServiceCollection AddInMemoryFeatures(this IServiceCollection services, Action<InMemoryFeaturesBuilder>? configure = null)
    {
        var builder = new InMemoryFeaturesBuilder();
        configure?.Invoke(builder);

        services.AddSingleton<IFeatureValueProvider>(new InMemoryGlobalFeatureValueProvider(builder.GlobalValues));
        services.AddSingleton<IFeatureValueProvider>(new InMemoryTenantFeatureValueProvider(builder.TenantValues));
        services.AddSingleton<IFeatureValueProvider>(new InMemoryUserFeatureValueProvider(builder.UserValues));

        return services;
    }
}
