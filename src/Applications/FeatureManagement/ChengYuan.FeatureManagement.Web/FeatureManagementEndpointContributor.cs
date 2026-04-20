using ChengYuan.AspNetCore;
using ChengYuan.Core.Application.Dtos;
using ChengYuan.Features;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ChengYuan.FeatureManagement;

public sealed class FeatureManagementEndpointContributor : IEndpointContributor
{
    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/features")
            .WithTags("FeatureManagement")
            .RequireAuthorization();

        group.MapGet("/", static async (IFeatureValueReader reader, CancellationToken cancellationToken) =>
            TypedResults.Ok(new ListResultDto<FeatureValueRecord>(await reader.GetListAsync(cancellationToken))))
            .RequireAuthorization(FeatureManagementPermissions.Features);

        group.MapPut("/", static async (FeatureValueRecord record, IFeatureValueManager manager, CancellationToken cancellationToken) =>
        {
            await manager.SetAsync(record, cancellationToken);
            return TypedResults.NoContent();
        }).RequireAuthorization(FeatureManagementPermissions.Features);

        group.MapDelete("/", RemoveFeatureAsync).RequireAuthorization(FeatureManagementPermissions.Features);
    }

    private static async Task<IResult> RemoveFeatureAsync(
        string name,
        string scope,
        IFeatureValueManager manager,
        Guid? tenantId = null,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<FeatureScope>(scope, ignoreCase: true, out var parsedScope))
        {
            return TypedResults.BadRequest(new { error = $"Invalid scope '{scope}'." });
        }

        await manager.RemoveAsync(name, parsedScope, tenantId, userId, cancellationToken);
        return TypedResults.NoContent();
    }
}
