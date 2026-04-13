using ChengYuan.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ChengYuan.Hosting;

public static class HostingCompositionExtensions
{
    /// <summary>
    /// Adds the ChengYuan modular framework to a generic host (console, worker, desktop).
    /// Uses the specified <typeparamref name="TRootModule"/> as the module graph root.
    /// </summary>
    public static IHostApplicationBuilder AddChengYuan<TRootModule>(
        this IHostApplicationBuilder builder,
        Action<ChengYuanBuilder> configure)
        where TRootModule : HostModule, new()
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);

        var cy = new ChengYuanBuilder(builder.Services);
        configure(cy);
        cy.Apply();

        builder.Services.AddModularApplication<TRootModule>();
        builder.Services.AddHostedService<ModularApplicationHostedService>();
        return builder;
    }

    /// <summary>
    /// Adds the ChengYuan modular framework to a raw <see cref="IServiceCollection"/>
    /// (MAUI, WPF, or any DI container). Caller is responsible for calling
    /// <see cref="IModularApplication.InitializeAsync"/> and <see cref="IModularApplication.ShutdownAsync"/>.
    /// </summary>
    public static IServiceCollection AddChengYuan<TRootModule>(
        this IServiceCollection services,
        Action<ChengYuanBuilder> configure)
        where TRootModule : HostModule, new()
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var cy = new ChengYuanBuilder(services);
        configure(cy);
        cy.Apply();

        services.AddModularApplication<TRootModule>();
        return services;
    }
}
