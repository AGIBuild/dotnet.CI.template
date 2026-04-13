using ChengYuan.Core.Modularity;
using ChengYuan.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.Hosting;

/// <summary>
/// Fluent builder for composing a ChengYuan modular application.
/// Each host project calls <see cref="AddModule{TModule}"/> to declare the
/// capabilities it needs; transitive <c>[DependsOn]</c> dependencies are resolved
/// automatically by the module graph.
/// </summary>
public sealed class ChengYuanBuilder
{
    /// <summary>
    /// The underlying service collection. Use this to register additional services
    /// or call provider-specific extensions (e.g. <c>builder.Services.UseSqlite(...)</c>).
    /// </summary>
    public IServiceCollection Services { get; }

    private Action<MultiTenancyBuilder>? _configureMultiTenancy;
    private bool _multiTenancyDisabled;

    internal ChengYuanBuilder(IServiceCollection services)
    {
        Services = services;
    }

    /// <summary>
    /// Registers a module and all of its transitive <c>[DependsOn]</c> dependencies.
    /// </summary>
    public ChengYuanBuilder AddModule<TModule>() where TModule : ModuleBase
    {
        Services.AddAdditionalModule<TModule>();
        return this;
    }

    /// <summary>
    /// Configures multi-tenancy with the provided builder callback.
    /// </summary>
    public ChengYuanBuilder ConfigureMultiTenancy(Action<MultiTenancyBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        _configureMultiTenancy = configure;
        return this;
    }

    /// <summary>
    /// Disables multi-tenancy for this host (e.g. CLI tools, desktop apps).
    /// </summary>
    public ChengYuanBuilder DisableMultiTenancy()
    {
        _multiTenancyDisabled = true;
        return this;
    }

    internal void Apply()
    {
        if (_multiTenancyDisabled)
        {
            Services.TryAddSingleton(new MultiTenancyOptions { IsEnabled = false });
        }
        else
        {
            Services.AddMultiTenancy(_configureMultiTenancy);
        }
    }
}
