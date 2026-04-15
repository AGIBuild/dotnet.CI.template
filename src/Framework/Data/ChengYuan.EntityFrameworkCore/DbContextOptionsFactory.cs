using ChengYuan.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ChengYuan.EntityFrameworkCore;

internal static class DbContextOptionsFactory
{
    public static void Configure<TDbContext>(IServiceProvider serviceProvider, DbContextOptionsBuilder optionsBuilder)
        where TDbContext : DbContext
    {
        var options = serviceProvider.GetRequiredService<IOptions<ChengYuanDbContextOptions>>().Value;
        var connectionString = ResolveConnectionString<TDbContext>(serviceProvider);

        if (options.ConfigureActions.TryGetValue(typeof(TDbContext), out var configureAction))
        {
            var context = new ChengYuanDbContextConfigurationContext<TDbContext>(serviceProvider, optionsBuilder)
            {
                ConnectionString = connectionString
            };
            ((Action<ChengYuanDbContextConfigurationContext<TDbContext>>)configureAction).Invoke(context);
        }
        else if (options.DefaultConfigureAction is not null)
        {
            var context = new ChengYuanDbContextConfigurationContext(serviceProvider, optionsBuilder)
            {
                ConnectionString = connectionString
            };
            options.DefaultConfigureAction.Invoke(context);
        }
        else
        {
            throw new InvalidOperationException(
                $"No database provider has been configured for DbContext '{typeof(TDbContext).FullName}'. " +
                "Call a provider extension such as UseSqlite(...) or configure UseDbContextOptions(...).");
        }

        AddInterceptors(serviceProvider, optionsBuilder);
        optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    private static string? ResolveConnectionString<TDbContext>(IServiceProvider serviceProvider)
        where TDbContext : DbContext
    {
        var resolver = serviceProvider.GetService<IConnectionStringResolver>();
        if (resolver is null)
        {
            return null;
        }

        var connectionStringName = ConnectionStringNameAttribute.GetNameOrDefault(typeof(TDbContext));
        return resolver.ResolveAsync(connectionStringName).GetAwaiter().GetResult();
    }

    private static void AddInterceptors(IServiceProvider serviceProvider, DbContextOptionsBuilder optionsBuilder)
    {
        var interceptors = serviceProvider.GetServices<IInterceptor>();
        optionsBuilder.AddInterceptors(interceptors);
    }
}
