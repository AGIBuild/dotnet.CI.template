using ChengYuan.AspNetCore;
using ChengYuan.Core.Application.Dtos;
using ChengYuan.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ChengYuan.SettingManagement;

public sealed class SettingManagementEndpointContributor : IEndpointContributor
{
    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/settings")
            .WithTags("SettingManagement")
            .RequireAuthorization();

        group.MapGet("/", static async (ISettingValueReader reader, CancellationToken cancellationToken) =>
            TypedResults.Ok(new ListResultDto<SettingValueRecord>(await reader.GetListAsync(cancellationToken))))
            .RequireAuthorization(SettingManagementPermissions.Settings);

        group.MapPut("/", static async (SettingValueRecord record, ISettingValueManager manager, CancellationToken cancellationToken) =>
        {
            await manager.SetAsync(record, cancellationToken);
            return TypedResults.NoContent();
        }).RequireAuthorization(SettingManagementPermissions.Settings);

        group.MapDelete("/", RemoveSettingAsync).RequireAuthorization(SettingManagementPermissions.Settings);
    }

    private static async Task<IResult> RemoveSettingAsync(
        string name,
        string scope,
        ISettingValueManager manager,
        Guid? tenantId = null,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<SettingScope>(scope, ignoreCase: true, out var parsedScope))
        {
            return TypedResults.BadRequest(new { error = $"Invalid scope '{scope}'." });
        }

        await manager.RemoveAsync(name, parsedScope, tenantId, userId, cancellationToken);
        return TypedResults.NoContent();
    }
}
