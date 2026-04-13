using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.ExceptionHandling;

public static class ExceptionHandlingServiceCollectionExtensions
{
    public static IServiceCollection AddExceptionHandling(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<IExceptionToErrorInfoConverter, DefaultExceptionToErrorInfoConverter>();

        return services;
    }

    public static IMvcBuilder AddChengYuanExceptionFilter(this IMvcBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddExceptionHandling();
        builder.AddMvcOptions(options => options.Filters.Add<ChengYuanExceptionFilter>());

        return builder;
    }
}
