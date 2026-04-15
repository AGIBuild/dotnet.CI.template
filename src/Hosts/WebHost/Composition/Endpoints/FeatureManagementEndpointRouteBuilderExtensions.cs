using ChengYuan.Core.Application.Dtos;
using ChengYuan.FeatureManagement;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ChengYuan.WebHost;

public static class FeatureManagementEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapFeatureManagementEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var group = endpoints.MapGroup("/features")
            .WithTags("FeatureManagement")
            .RequireAuthorization();

        group.MapGet("/", static async (IFeatureValueReader reader, CancellationToken cancellationToken) =>
            TypedResults.Ok(new ListResultDto<FeatureValueRecord>(await reader.GetListAsync(cancellationToken))));

        group.MapPut("/", static async (FeatureValueRecord record, IFeatureValueManager manager, CancellationToken cancellationToken) =>
        {
            await manager.SetAsync(record, cancellationToken);
            return TypedResults.NoContent();
        });

        group.MapDelete("/", RemoveFeatureAsync);

        return endpoints;
    }

    private static async Task<IResult> RemoveFeatureAsync(
        string name,
        string scope,
        IFeatureValueManager manager,
        Guid? tenantId = null,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<ChengYuan.Features.FeatureScope>(scope, ignoreCase: true, out var parsedScope))
        {
            return TypedResults.BadRequest(new { error = $"Invalid scope '{scope}'." });
        }

        await manager.RemoveAsync(name, parsedScope, tenantId, userId, cancellationToken);
        return TypedResults.NoContent();
    }
}
