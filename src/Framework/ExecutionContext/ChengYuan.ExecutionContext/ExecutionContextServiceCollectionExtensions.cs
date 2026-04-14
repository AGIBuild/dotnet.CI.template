using ChengYuan.Core.Guids;
using ChengYuan.Core.Timing;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.ExecutionContext;

public static class ExecutionContextServiceCollectionExtensions
{
    public static IServiceCollection AddExecutionContext(this IServiceCollection services)
    {
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IGuidGenerator, DefaultGuidGenerator>();
        services.AddSingleton<ICurrentCorrelationAccessor, CurrentCorrelationAccessor>();
        services.AddSingleton<ICurrentCorrelation>(serviceProvider => serviceProvider.GetRequiredService<ICurrentCorrelationAccessor>());
        services.AddSingleton<ICurrentUserAccessor, CurrentUserAccessor>();
        services.AddSingleton<ICurrentUser>(serviceProvider => serviceProvider.GetRequiredService<ICurrentUserAccessor>());
        return services;
    }
}
