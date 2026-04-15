using ChengYuan.Core.Application.Dtos;
using ChengYuan.TenantManagement;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ChengYuan.WebHost;

public static class TenantManagementEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapTenantManagementEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var group = endpoints.MapGroup("/tenants")
            .WithTags("TenantManagement")
            .RequireAuthorization();

        group.MapGet("/", static async (ITenantReader reader, CancellationToken cancellationToken) =>
            TypedResults.Ok(new ListResultDto<TenantRecord>(await reader.GetListAsync(cancellationToken))));

        group.MapGet("/{tenantId:guid}", GetTenantByIdAsync);
        group.MapPost("/", CreateTenantAsync);
        group.MapPut("/", SetTenantAsync);
        group.MapDelete("/{tenantId:guid}", DeleteTenantAsync);

        return endpoints;
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
