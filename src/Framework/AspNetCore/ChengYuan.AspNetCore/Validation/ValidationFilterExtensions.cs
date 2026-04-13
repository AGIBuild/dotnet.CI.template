using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.AspNetCore;

public static class ValidationFilterExtensions
{
    public static IMvcBuilder AddAutoValidation(this IMvcBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddMvcOptions(options => options.Filters.Add<ValidationActionFilter>(order: -1000));

        return builder;
    }
}
