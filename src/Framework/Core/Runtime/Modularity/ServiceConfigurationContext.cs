using ChengYuan.Core.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Core.Modularity;

public sealed class ServiceConfigurationContext
{
    internal ServiceConfigurationContext(IServiceCollection services, IInitLoggerFactory initLoggerFactory, IConfiguration? configuration = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(initLoggerFactory);

        Services = services;
        InitLoggerFactory = initLoggerFactory;
        Configuration = configuration;
        Items = new Dictionary<string, object?>();
    }

    public IServiceCollection Services { get; }

    public IInitLoggerFactory InitLoggerFactory { get; }

    public IConfiguration? Configuration { get; }

    public IDictionary<string, object?> Items { get; }

    public object? this[string key]
    {
        get => Items.TryGetValue(key, out var value) ? value : null;
        set => Items[key] = value;
    }
}
