using ChengYuan.AspNetCore;
using ChengYuan.Core.Application.Dtos;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ChengYuan.TenantManagement;

public sealed class TenantManagementEndpointContributor : IEndpointContributor
{
    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/tenants")
            .WithTags("TenantManagement")
            .RequireAuthorization();

        group.MapGet("/", static async (ITenantReader reader, CancellationToken cancellationToken) =>
            TypedResults.Ok(new ListResultDto<TenantRecord>(await reader.GetListAsync(cancellationToken))))
            .RequireAuthorization(TenantManagementPermissions.Tenants);

        group.MapGet("/{tenantId:guid}", GetTenantByIdAsync).RequireAuthorization(TenantManagementPermissions.Tenants);
        group.MapPost("/", CreateTenantAsync).RequireAuthorization(TenantManagementPermissions.TenantsCreate);
        group.MapPut("/", SetTenantAsync).RequireAuthorization(TenantManagementPermissions.TenantsUpdate);
        group.MapDelete("/{tenantId:guid}", DeleteTenantAsync).RequireAuthorization(TenantManagementPermissions.TenantsDelete);
    }

    private static async Task<IResult> GetTenantByIdAsync(
        Guid tenantId,
        ITenantReader reader,
        CancellationToken cancellationToken)
    {
        var tenant = await reader.FindByIdAsync(tenantId, cancellationToken);
        return tenant is null ? TypedResults.NotFound() : TypedResults.Ok(tenant);
    }

    private static async Task<IResult> CreateTenantAsync(
        CreateTenantRequest request,
        ITenantManager manager,
        CancellationToken cancellationToken)
    {
        var tenant = await manager.CreateAsync(request.Name, request.IsActive, cancellationToken);
        return TypedResults.Created($"/tenants/{tenant.Id}", tenant);
    }

    private static async Task<IResult> SetTenantAsync(
        TenantRecord record,
        ITenantManager manager,
        CancellationToken cancellationToken)
    {
        await manager.SetAsync(record, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<IResult> DeleteTenantAsync(
        Guid tenantId,
        ITenantManager manager,
        CancellationToken cancellationToken)
    {
        await manager.RemoveAsync(tenantId, cancellationToken);
        return TypedResults.NoContent();
    }
}
