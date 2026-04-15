using ChengYuan.Core.Application.Dtos;
using ChengYuan.SettingManagement;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ChengYuan.WebHost;

public static class SettingManagementEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapSettingManagementEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var group = endpoints.MapGroup("/settings")
            .WithTags("SettingManagement")
            .RequireAuthorization();

        group.MapGet("/", static async (ISettingValueReader reader, CancellationToken cancellationToken) =>
            TypedResults.Ok(new ListResultDto<SettingValueRecord>(await reader.GetListAsync(cancellationToken))));

        group.MapPut("/", static async (SettingValueRecord record, ISettingValueManager manager, CancellationToken cancellationToken) =>
        {
            await manager.SetAsync(record, cancellationToken);
            return TypedResults.NoContent();
        });

        group.MapDelete("/", RemoveSettingAsync);

        return endpoints;
    }

    private static async Task<IResult> RemoveSettingAsync(
        string name,
        string scope,
        ISettingValueManager manager,
        Guid? tenantId = null,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<ChengYuan.Settings.SettingScope>(scope, ignoreCase: true, out var parsedScope))
        {
            return TypedResults.BadRequest(new { error = $"Invalid scope '{scope}'." });
        }

        await manager.RemoveAsync(name, parsedScope, tenantId, userId, cancellationToken);
        return TypedResults.NoContent();
    }
}
