using ChengYuan.Core.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Core.Modularity;

public sealed class ServiceConfigurationContext
{
    internal ServiceConfigurationContext(IServiceCollection services, IInitLoggerFactory initLoggerFactory)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(initLoggerFactory);

        Services = services;
        InitLoggerFactory = initLoggerFactory;
        Items = new Dictionary<string, object?>();
    }

    public IServiceCollection Services { get; }

    public IInitLoggerFactory InitLoggerFactory { get; }

    public IDictionary<string, object?> Items { get; }

    public object? this[string key]
    {
        get => Items.TryGetValue(key, out var value) ? value : null;
        set => Items[key] = value;
    }
}
