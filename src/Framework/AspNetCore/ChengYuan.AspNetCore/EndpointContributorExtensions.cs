using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.AspNetCore;

public static class EndpointContributorExtensions
{
    public static IEndpointRouteBuilder MapModuleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var contributors = endpoints.ServiceProvider.GetServices<IEndpointContributor>();

        foreach (var contributor in contributors)
        {
            contributor.MapEndpoints(endpoints);
        }

        return endpoints;
    }
}
