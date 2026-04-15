using ChengYuan.Core.Application.Dtos;
using ChengYuan.PermissionManagement;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ChengYuan.WebHost;

public static class PermissionManagementEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapPermissionManagementEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var group = endpoints.MapGroup("/permissions")
            .WithTags("PermissionManagement")
            .RequireAuthorization();

        group.MapGet("/", static async (IPermissionGrantReader reader, CancellationToken cancellationToken) =>
            TypedResults.Ok(new ListResultDto<PermissionGrantRecord>(await reader.GetListAsync(cancellationToken))));

        group.MapPut("/", static async (PermissionGrantRecord record, IPermissionGrantManager manager, CancellationToken cancellationToken) =>
        {
            await manager.SetAsync(record, cancellationToken);
            return TypedResults.NoContent();
        });

        group.MapDelete("/", RemovePermissionAsync);

        return endpoints;
    }

    private static async Task<IResult> RemovePermissionAsync(
        string name,
        string scope,
        IPermissionGrantManager manager,
        Guid? tenantId = null,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<ChengYuan.Authorization.PermissionScope>(scope, ignoreCase: true, out var parsedScope))
        {
            return TypedResults.BadRequest(new { error = $"Invalid scope '{scope}'." });
        }

        await manager.RemoveAsync(name, parsedScope, tenantId, userId, cancellationToken);
        return TypedResults.NoContent();
    }
}
