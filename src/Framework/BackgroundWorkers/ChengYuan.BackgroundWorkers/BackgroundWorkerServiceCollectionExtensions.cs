using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace ChengYuan.BackgroundWorkers;

public static class BackgroundWorkerServiceCollectionExtensions
{
    public static IServiceCollection AddBackgroundWorkerManager(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<IBackgroundWorkerManager, BackgroundWorkerManager>();
        services.AddHostedService<BackgroundWorkerManagerHostedService>();

        return services;
    }

    public static IServiceCollection AddBackgroundWorker<TWorker>(this IServiceCollection services)
        where TWorker : class, IBackgroundWorker
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<TWorker>();
        services.AddHostedService<BackgroundWorkerRegistrar<TWorker>>();

        return services;
    }

    private sealed class BackgroundWorkerManagerHostedService(IBackgroundWorkerManager manager) : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return manager.StartAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return manager.StopAsync(cancellationToken);
        }
    }

    private sealed class BackgroundWorkerRegistrar<TWorker>(
        IBackgroundWorkerManager manager,
        TWorker worker) : IHostedService
        where TWorker : class, IBackgroundWorker
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return manager.AddAsync(worker, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
