using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Interceptors;

public static class InterceptorServiceCollectionExtensions
{
    public static IServiceCollection AddInterceptors(this IServiceCollection services)
    {
        services.AddTransient<InterceptorPipeline>();
        return services;
    }
}
