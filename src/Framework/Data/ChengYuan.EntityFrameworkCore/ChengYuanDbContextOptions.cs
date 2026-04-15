using Microsoft.EntityFrameworkCore;

namespace ChengYuan.EntityFrameworkCore;

public sealed class ChengYuanDbContextOptions
{
    internal Action<ChengYuanDbContextConfigurationContext>? DefaultConfigureAction { get; set; }

    internal Dictionary<Type, object> ConfigureActions { get; } = new();

    internal HashSet<Type> RegisteredDbContextTypes { get; } = [];

    public void Configure(Action<ChengYuanDbContextConfigurationContext> configureAction)
    {
        ArgumentNullException.ThrowIfNull(configureAction);

        DefaultConfigureAction = configureAction;
    }

    public void Configure<TDbContext>(Action<ChengYuanDbContextConfigurationContext<TDbContext>> configureAction)
        where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(configureAction);

        ConfigureActions[typeof(TDbContext)] = configureAction;
    }

    internal bool IsConfiguredDefault() => DefaultConfigureAction is not null;

    internal bool IsConfigured<TDbContext>() where TDbContext : DbContext
        => IsConfigured(typeof(TDbContext));

    internal bool IsConfigured(Type dbContextType) => ConfigureActions.ContainsKey(dbContextType);
}
