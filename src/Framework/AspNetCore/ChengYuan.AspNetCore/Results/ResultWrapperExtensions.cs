using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.AspNetCore;

public static class ResultWrapperExtensions
{
    public static IMvcBuilder AddResultWrapper(this IMvcBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddMvcOptions(options => options.Filters.Add<ResultWrapperFilter>());

        return builder;
    }
}
