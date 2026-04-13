using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;

namespace ChengYuan.HttpResilience;

public static class ResilientHttpClientExtensions
{
    public static IHttpClientBuilder AddChengYuanResilience(this IHttpClientBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddStandardResilienceHandler();

        return builder;
    }

    public static IHttpClientBuilder AddChengYuanResilience(
        this IHttpClientBuilder builder,
        Action<HttpStandardResilienceOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);

        builder.AddStandardResilienceHandler(configure);

        return builder;
    }
}
