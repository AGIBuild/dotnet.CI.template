using ChengYuan.AspNetCore;
using ChengYuan.Authorization;
using ChengYuan.Core.Application.Dtos;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ChengYuan.PermissionManagement;

public sealed class PermissionManagementEndpointContributor : IEndpointContributor
{
    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/permissions")
            .WithTags("PermissionManagement")
            .RequireAuthorization();

        group.MapGet("/", static async (IPermissionGrantReader reader, CancellationToken cancellationToken) =>
            TypedResults.Ok(new ListResultDto<PermissionGrantRecord>(await reader.GetListAsync(cancellationToken))))
            .RequireAuthorization(PermissionManagementPermissions.Permissions);

        group.MapPut("/", static async (PermissionGrantRecord record, IPermissionGrantManager manager, CancellationToken cancellationToken) =>
        {
            await manager.SetAsync(record, cancellationToken);
            return TypedResults.NoContent();
        }).RequireAuthorization(PermissionManagementPermissions.Permissions);

        group.MapDelete("/", RemovePermissionAsync).RequireAuthorization(PermissionManagementPermissions.Permissions);
    }

    private static async Task<IResult> RemovePermissionAsync(
        string name,
        string scope,
        IPermissionGrantManager manager,
        Guid? tenantId = null,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<PermissionScope>(scope, ignoreCase: true, out var parsedScope))
        {
            return TypedResults.BadRequest(new { error = $"Invalid scope '{scope}'." });
        }

        await manager.RemoveAsync(name, parsedScope, tenantId, userId, cancellationToken);
        return TypedResults.NoContent();
    }
}
