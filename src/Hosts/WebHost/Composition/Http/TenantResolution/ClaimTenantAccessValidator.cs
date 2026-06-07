using System.Security.Claims;
using ChengYuan.Identity;
using ChengYuan.MultiTenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;

namespace ChengYuan.WebHost;

internal sealed class ClaimTenantAccessValidator(
    MultiTenancyOptions options,
    IUserTenantMembershipReader membershipReader) : IHttpTenantAccessValidator
{
    public ValueTask<bool> CanAccessAsync(
        HttpContext httpContext,
        TenantResolveResult tenant,
        CancellationToken cancellationToken = default)
    {
        if (tenant.TenantId is not Guid tenantId)
        {
            return ValueTask.FromResult(true);
        }

        if (httpContext.User.Identity?.IsAuthenticated != true)
        {
            return ValueTask.FromResult(true);
        }

        if (HasConflictingClientTenantCandidate(httpContext, tenantId, tenant.TenantName))
        {
            return ValueTask.FromResult(false);
        }

        var hasMatchingTenantClaim = false;
        foreach (var claimType in options.ClaimTypes)
        {
            foreach (var claim in httpContext.User.FindAll(claimType))
            {
                if (ClaimMatchesTenant(claim.Value, tenantId, tenant.TenantName))
                {
                    hasMatchingTenantClaim = true;
                    break;
                }
            }

            if (hasMatchingTenantClaim)
            {
                break;
            }
        }

        if (!hasMatchingTenantClaim)
        {
            return ValueTask.FromResult(false);
        }

        var userId = GetUserId(httpContext.User);
        if (userId is not Guid resolvedUserId)
        {
            return ValueTask.FromResult(false);
        }

        return membershipReader.IsActiveMemberAsync(resolvedUserId, tenantId, cancellationToken);
    }

    private bool HasConflictingClientTenantCandidate(HttpContext httpContext, Guid tenantId, string? tenantName)
    {
        foreach (var candidate in EnumerateClientTenantCandidates(httpContext))
        {
            if (!ClaimMatchesTenant(candidate, tenantId, tenantName))
            {
                return true;
            }
        }

        return false;
    }

    private IEnumerable<string> EnumerateClientTenantCandidates(HttpContext httpContext)
    {
        foreach (var headerName in DistinctNames(options.HeaderName, options.TenantKey))
        {
            if (httpContext.Request.Headers.TryGetValue(headerName, out var headerValues))
            {
                foreach (var value in headerValues)
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        yield return value!;
                    }
                }
            }
        }

        foreach (var queryStringKey in DistinctNames(options.QueryStringKey, options.TenantKey))
        {
            if (httpContext.Request.Query.TryGetValue(queryStringKey, out var queryValues))
            {
                foreach (var value in queryValues)
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        yield return value!;
                    }
                }
            }
        }

        foreach (var routeKey in DistinctNames(options.RouteKey, options.TenantKey))
        {
            if (httpContext.Request.RouteValues.TryGetValue(routeKey, out var routeValue)
                && !string.IsNullOrWhiteSpace(routeValue?.ToString()))
            {
                yield return routeValue.ToString()!;
            }
        }

        foreach (var cookieName in DistinctNames(options.CookieName, options.TenantKey))
        {
            if (httpContext.Request.Cookies.TryGetValue(cookieName, out var cookieValue)
                && !string.IsNullOrWhiteSpace(cookieValue))
            {
                yield return cookieValue;
            }
        }

        foreach (var pattern in options.DomainPatterns)
        {
            var host = httpContext.Request.Host.Host;
            if (string.IsNullOrWhiteSpace(host))
            {
                continue;
            }

            var tenantName = ExtractTenantFromHost(host, pattern);
            if (!string.IsNullOrWhiteSpace(tenantName))
            {
                yield return tenantName;
            }
        }
    }

    private static IEnumerable<string> DistinctNames(params string[] names)
    {
        return names
            .Where(static name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }

    private static bool ClaimMatchesTenant(string claimValue, Guid tenantId, string? tenantName)
    {
        if (Guid.TryParse(claimValue, out var claimTenantId))
        {
            return claimTenantId == tenantId;
        }

        return !string.IsNullOrWhiteSpace(tenantName)
            && string.Equals(claimValue, tenantName, StringComparison.OrdinalIgnoreCase);
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var value = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(value, out var userId) ? userId : null;
    }

    private static string? ExtractTenantFromHost(string host, string pattern)
    {
        var regexPattern = "^" + System.Text.RegularExpressions.Regex.Escape(pattern).Replace("\\{0}", "([^.]+)") + "$";
        var match = System.Text.RegularExpressions.Regex.Match(host, regexPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        return match.Success && match.Groups.Count > 1 ? match.Groups[1].Value : null;
    }
}
