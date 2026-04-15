using System;
using ChengYuan.Core.Data;
using ChengYuan.ExecutionContext;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.MultiTenancy;

public static class MultiTenancyServiceCollectionExtensions
{
    /// <summary>
    /// Registers multi-tenancy services for this application.
    /// Web-specific resolution sources are activated by host composition layers.
    /// </summary>
    public static IServiceCollection AddMultiTenancy(
        this IServiceCollection services,
        Action<MultiTenancyBuilder>? configure = null)
    {
        var builder = new MultiTenancyBuilder(services);
        configure?.Invoke(builder);

        services.AddSingleton(builder.Options);

        // Store the error handler for middleware consumption
        if (builder.ErrorHandler is not null)
        {
            services.AddSingleton(new TenantResolutionErrorHandlerHolder(builder.ErrorHandler));
        }

        services.Configure<TenantResolveOptions>(resolveOptions =>
        {
            if (builder.Options.FallbackTenantId.HasValue)
            {
                resolveOptions.FallbackTenantId = builder.Options.FallbackTenantId;
                resolveOptions.FallbackTenantName = builder.Options.FallbackTenantName;
            }
        });

        AddMultiTenancyCore(services);

        return services;
    }

    internal static IServiceCollection AddMultiTenancyCore(this IServiceCollection services)
    {
        services.TryAddSingleton<ICurrentTenantAccessor, CurrentTenantAccessor>();
        services.TryAddSingleton<ICurrentTenant>(serviceProvider => serviceProvider.GetRequiredService<ICurrentTenantAccessor>());
        services.Replace(ServiceDescriptor.Singleton<IDataTenantProvider>(
            serviceProvider => new CurrentTenantDataTenantProvider(serviceProvider.GetRequiredService<ICurrentTenant>())));

        // Override default connection string resolver with tenant-aware version
        services.Replace(ServiceDescriptor.Transient<IConnectionStringResolver, MultiTenantConnectionStringResolver>());

        // Resolution pipeline
        services.AddOptions<TenantResolveOptions>();
        services.TryAddTransient<ITenantResolver, TenantResolver>();

        // Default no-op store; application layer can replace with a real implementation
        services.TryAddSingleton<ITenantResolutionStore, NoOpTenantResolutionStore>();

        return services;
    }
}
