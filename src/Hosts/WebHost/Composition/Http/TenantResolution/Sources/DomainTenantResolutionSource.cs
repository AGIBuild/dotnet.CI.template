using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ChengYuan.MultiTenancy;
using Microsoft.AspNetCore.Http;

namespace ChengYuan.WebHost;

/// <summary>
/// Resolves tenant from the request host using ordered domain/subdomain patterns.
/// Each pattern should contain a <c>{0}</c> placeholder that maps to the tenant segment.
/// Example: <c>"{0}.myapp.com"</c>
/// </summary>
internal sealed class DomainTenantResolutionSource(MultiTenancyOptions options) : ITenantResolutionSource
{
    public int Order => 600;

    public bool IsAvailable(IServiceProvider serviceProvider)
    {
        if (options.DomainPatterns.Count == 0)
        {
            return false;
        }

        var accessor = serviceProvider.GetService(typeof(IHttpContextAccessor)) as IHttpContextAccessor;
        return accessor?.HttpContext is not null;
    }

    public ValueTask PopulateAsync(
        TenantResolveContext context,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        if (context.HasResolved)
        {
            return ValueTask.CompletedTask;
        }

        var accessor = serviceProvider.GetService(typeof(IHttpContextAccessor)) as IHttpContextAccessor;
        var httpContext = accessor?.HttpContext;
        if (httpContext is null)
        {
            return ValueTask.CompletedTask;
        }

        var host = httpContext.Request.Host.Host;
        if (string.IsNullOrWhiteSpace(host))
        {
            return ValueTask.CompletedTask;
        }

        foreach (var pattern in options.DomainPatterns)
        {
            var tenant = ExtractTenantFromHost(host, pattern);
            if (string.IsNullOrWhiteSpace(tenant))
            {
                continue;
            }

            if (Guid.TryParse(tenant, out var tenantId))
            {
                context.TenantId = tenantId;
                context.HasResolved = true;
                context.SourceName = nameof(DomainTenantResolutionSource);
                return ValueTask.CompletedTask;
            }

            context.TenantName = tenant;
            context.SourceName = nameof(DomainTenantResolutionSource);
            return ValueTask.CompletedTask;
        }

        return ValueTask.CompletedTask;
    }

    private static string? ExtractTenantFromHost(string host, string pattern)
    {
        var regexPattern = "^" + Regex.Escape(pattern).Replace("\\{0}", "([^.]+)") + "$";

        var match = Regex.Match(host, regexPattern, RegexOptions.IgnoreCase);
        return match.Success && match.Groups.Count > 1 ? match.Groups[1].Value : null;
    }
}
