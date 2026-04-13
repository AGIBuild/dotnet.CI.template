using ChengYuan.Caching;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ChengYuan.HealthChecks;

public sealed class CacheHealthCheck(IChengYuanCache cache) : IHealthCheck
{
    private static readonly ChengYuanCacheKey ProbeKey = new("health:probe", ChengYuanCacheScope.Global);

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await cache.SetAsync(ProbeKey, "ok", cancellationToken: cancellationToken);
            var value = await cache.GetAsync<string>(ProbeKey, cancellationToken);
            await cache.RemoveAsync(ProbeKey, cancellationToken);

            return value is "ok"
                ? HealthCheckResult.Healthy("Cache is operational.")
                : HealthCheckResult.Degraded("Cache returned unexpected value.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Cache health check failed.", ex);
        }
    }
}
