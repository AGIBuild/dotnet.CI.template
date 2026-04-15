using ChengYuan.Core.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ChengYuan.WebHost;

internal sealed partial class DataSeedingHostedService(
    IServiceProvider serviceProvider,
    ILogger<DataSeedingHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        LogSeedingStarted(logger);

        await using var scope = serviceProvider.CreateAsyncScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
        await seeder.SeedAsync(new DataSeedContext(), cancellationToken);

        LogSeedingCompleted(logger);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    [LoggerMessage(Level = LogLevel.Information, Message = "Running data seed contributors...")]
    private static partial void LogSeedingStarted(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Data seeding completed.")]
    private static partial void LogSeedingCompleted(ILogger logger);
}
