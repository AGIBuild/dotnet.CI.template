using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.HealthChecks;

public static class HealthCheckServiceCollectionExtensions
{
    public static IHealthChecksBuilder AddChengYuanDbContextCheck<TDbContext>(
        this IHealthChecksBuilder builder,
        string? name = null,
        params string[] tags)
        where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddCheck<DbContextHealthCheck<TDbContext>>(
            name ?? $"db:{typeof(TDbContext).Name}",
            tags: tags.Length > 0 ? tags : ["ready"]);
    }

    public static IHealthChecksBuilder AddChengYuanCacheCheck(
        this IHealthChecksBuilder builder,
        string? name = null,
        params string[] tags)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddCheck<CacheHealthCheck>(
            name ?? "cache",
            tags: tags.Length > 0 ? tags : ["ready"]);
    }
}
