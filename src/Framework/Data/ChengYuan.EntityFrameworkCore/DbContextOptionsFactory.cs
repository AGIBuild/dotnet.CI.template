using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ChengYuan.EntityFrameworkCore;

internal static class DbContextOptionsFactory
{
    public static void Configure<TDbContext>(IServiceProvider serviceProvider, DbContextOptionsBuilder optionsBuilder)
        where TDbContext : DbContext
    {
        var options = serviceProvider.GetRequiredService<IOptions<ChengYuanDbContextOptions>>().Value;

        if (options.ConfigureActions.TryGetValue(typeof(TDbContext), out var configureAction))
        {
            var context = new ChengYuanDbContextConfigurationContext<TDbContext>(serviceProvider, optionsBuilder);
            ((Action<ChengYuanDbContextConfigurationContext<TDbContext>>)configureAction).Invoke(context);
        }
        else if (options.DefaultConfigureAction is not null)
        {
            var context = new ChengYuanDbContextConfigurationContext(serviceProvider, optionsBuilder);
            options.DefaultConfigureAction.Invoke(context);
        }
        else
        {
            throw new InvalidOperationException(
                $"No database provider has been configured for DbContext '{typeof(TDbContext).FullName}'. " +
                "Call a provider extension such as UseSqlite(...) or configure UseDbContextOptions(...).");
        }
    }
}
