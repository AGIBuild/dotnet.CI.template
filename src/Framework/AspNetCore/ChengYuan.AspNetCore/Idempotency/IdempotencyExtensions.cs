using System;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.AspNetCore;

public static class IdempotencyExtensions
{
    public static IMvcBuilder AddIdempotency(this IMvcBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddMvcOptions(options => options.Filters.Add<IdempotencyFilter>());

        return builder;
    }
}
