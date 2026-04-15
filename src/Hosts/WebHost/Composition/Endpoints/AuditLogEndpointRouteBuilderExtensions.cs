using ChengYuan.AuditLogging;
using ChengYuan.Core.Application.Dtos;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ChengYuan.WebHost;

public static class AuditLogEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapAuditLogEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var group = endpoints.MapGroup("/audit-logs")
            .WithTags("AuditLogging")
            .RequireAuthorization();

        group.MapGet("/", static async (IAuditLogReader reader, CancellationToken cancellationToken) =>
            TypedResults.Ok(new ListResultDto<AuditLogRecord>(await reader.GetListAsync(cancellationToken))));

        return endpoints;
    }
}
