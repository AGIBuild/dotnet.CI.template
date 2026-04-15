using ChengYuan.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.BackgroundJobs;

public static class BackgroundJobPersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddBackgroundJobPersistence(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddPersistenceModule<BackgroundJobDbContext, IBackgroundJobStore, IBackgroundJobStore, EfBackgroundJobStore>();

        return services;
    }
}
