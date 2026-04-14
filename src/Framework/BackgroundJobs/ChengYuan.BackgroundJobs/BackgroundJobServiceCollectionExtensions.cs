using ChengYuan.BackgroundWorkers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.BackgroundJobs;

public static class BackgroundJobServiceCollectionExtensions
{
    public static IServiceCollection AddBackgroundJobs(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<IBackgroundJobStore, InMemoryBackgroundJobStore>();
        services.TryAddTransient<IBackgroundJobManager, DefaultBackgroundJobManager>();
        services.AddBackgroundWorker<BackgroundJobExecutionWorker>();

        return services;
    }
}
