using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChengYuan.EntityFrameworkCore;

internal sealed partial class DatabaseMigrationHostedService(
    IServiceProvider serviceProvider,
    IOptions<ChengYuanDbContextOptions> options,
    ILogger<DatabaseMigrationHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var dbContextTypes = options.Value.RegisteredDbContextTypes;
        if (dbContextTypes.Count == 0)
        {
            return;
        }

        await using var scope = serviceProvider.CreateAsyncScope();

        foreach (var dbContextType in dbContextTypes)
        {
            LogApplyingMigrations(logger, dbContextType.Name);
            var dbContext = (DbContext)scope.ServiceProvider.GetRequiredService(dbContextType);
            await dbContext.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
        }

        LogAllMigrationsApplied(logger);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    [LoggerMessage(Level = LogLevel.Information, Message = "Applying migrations for {DbContextName}...")]
    private static partial void LogApplyingMigrations(ILogger logger, string dbContextName);

    [LoggerMessage(Level = LogLevel.Information, Message = "All database migrations applied successfully.")]
    private static partial void LogAllMigrationsApplied(ILogger logger);
}
