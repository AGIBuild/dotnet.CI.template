using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ChengYuan.HealthChecks;

public sealed class DbContextHealthCheck<TDbContext>(TDbContext dbContext) : IHealthCheck
    where TDbContext : DbContext
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

            return canConnect
                ? HealthCheckResult.Healthy($"{typeof(TDbContext).Name} is reachable.")
                : HealthCheckResult.Unhealthy($"{typeof(TDbContext).Name} cannot connect.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"{typeof(TDbContext).Name} health check failed.", ex);
        }
    }
}
