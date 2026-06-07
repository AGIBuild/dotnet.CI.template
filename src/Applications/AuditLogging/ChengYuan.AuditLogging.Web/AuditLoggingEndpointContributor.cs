using ChengYuan.AspNetCore;
using ChengYuan.Core.Application.Dtos;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ChengYuan.AuditLogging;

public sealed class AuditLoggingEndpointContributor : IEndpointContributor
{
    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/audit-logs")
            .WithTags("AuditLogging")
            .RequireAuthorization();

        group.MapGet("/", static async (IAuditLogManager manager, CancellationToken cancellationToken) =>
            TypedResults.Ok(new ListResultDto<AuditLogRecord>(await manager.GetListAsync(cancellationToken))))
            .RequireAuthorization(AuditLoggingPermissions.AuditLogs);
    }
}
