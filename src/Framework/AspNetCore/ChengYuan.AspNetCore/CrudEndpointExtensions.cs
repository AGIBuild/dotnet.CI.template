using System;
using System.Diagnostics.CodeAnalysis;
using ChengYuan.Application.Services;
using ChengYuan.Core.Application.Dtos;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.AspNetCore;

public static class CrudEndpointExtensions
{
    [SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035", Justification = "Analyzer bug with generic route handlers")]
    public static RouteGroupBuilder MapCrudEndpoints<TAppService, TGetOutputDto, TGetListOutputDto, TKey, TGetListInput, TCreateInput, TUpdateInput>(
        this IEndpointRouteBuilder endpoints,
        string pattern)
        where TAppService : class, ICrudAppService<TGetOutputDto, TGetListOutputDto, TKey, TGetListInput, TCreateInput, TUpdateInput>
        where TKey : IParsable<TKey>
    {
        var group = endpoints.MapGroup(pattern);

        group.MapGet("/{id}", GetByIdAsync<TAppService, TGetOutputDto, TGetListOutputDto, TKey, TGetListInput, TCreateInput, TUpdateInput>);
        group.MapGet("/", GetListAsync<TAppService, TGetOutputDto, TGetListOutputDto, TKey, TGetListInput, TCreateInput, TUpdateInput>);
        group.MapPost("/", CreateAsync<TAppService, TGetOutputDto, TGetListOutputDto, TKey, TGetListInput, TCreateInput, TUpdateInput>);
        group.MapPut("/{id}", UpdateAsync<TAppService, TGetOutputDto, TGetListOutputDto, TKey, TGetListInput, TCreateInput, TUpdateInput>);
        group.MapDelete("/{id}", DeleteAsync<TAppService, TGetOutputDto, TGetListOutputDto, TKey, TGetListInput, TCreateInput, TUpdateInput>);

        return group;
    }

    public static RouteGroupBuilder MapCrudEndpoints<TAppService, TEntityDto, TKey>(
        this IEndpointRouteBuilder endpoints,
        string pattern)
        where TAppService : class, ICrudAppService<TEntityDto, TKey>
        where TKey : IParsable<TKey>
    {
        return endpoints.MapCrudEndpoints<TAppService, TEntityDto, TEntityDto, TKey, PagedAndSortedResultRequestDto, TEntityDto, TEntityDto>(pattern);
    }

    private static async Task<IResult> GetByIdAsync<TAppService, TGetOutputDto, TGetListOutputDto, TKey, TGetListInput, TCreateInput, TUpdateInput>(
        TKey id, TAppService service, CancellationToken cancellationToken)
        where TAppService : class, ICrudAppService<TGetOutputDto, TGetListOutputDto, TKey, TGetListInput, TCreateInput, TUpdateInput>
        where TKey : IParsable<TKey>
    {
        return Results.Ok(await service.GetAsync(id, cancellationToken));
    }

    private static async Task<IResult> GetListAsync<TAppService, TGetOutputDto, TGetListOutputDto, TKey, TGetListInput, TCreateInput, TUpdateInput>(
        [AsParameters] TGetListInput input, TAppService service, CancellationToken cancellationToken)
        where TAppService : class, ICrudAppService<TGetOutputDto, TGetListOutputDto, TKey, TGetListInput, TCreateInput, TUpdateInput>
        where TKey : IParsable<TKey>
    {
        return Results.Ok(await service.GetListAsync(input!, cancellationToken));
    }

    private static async Task<IResult> CreateAsync<TAppService, TGetOutputDto, TGetListOutputDto, TKey, TGetListInput, TCreateInput, TUpdateInput>(
        TCreateInput input, TAppService service, CancellationToken cancellationToken)
        where TAppService : class, ICrudAppService<TGetOutputDto, TGetListOutputDto, TKey, TGetListInput, TCreateInput, TUpdateInput>
        where TKey : IParsable<TKey>
    {
        return Results.Created(string.Empty, await service.CreateAsync(input!, cancellationToken));
    }

    private static async Task<IResult> UpdateAsync<TAppService, TGetOutputDto, TGetListOutputDto, TKey, TGetListInput, TCreateInput, TUpdateInput>(
        TKey id, TUpdateInput input, TAppService service, CancellationToken cancellationToken)
        where TAppService : class, ICrudAppService<TGetOutputDto, TGetListOutputDto, TKey, TGetListInput, TCreateInput, TUpdateInput>
        where TKey : IParsable<TKey>
    {
        return Results.Ok(await service.UpdateAsync(id, input!, cancellationToken));
    }

    private static async Task<IResult> DeleteAsync<TAppService, TGetOutputDto, TGetListOutputDto, TKey, TGetListInput, TCreateInput, TUpdateInput>(
        TKey id, TAppService service, CancellationToken cancellationToken)
        where TAppService : class, ICrudAppService<TGetOutputDto, TGetListOutputDto, TKey, TGetListInput, TCreateInput, TUpdateInput>
        where TKey : IParsable<TKey>
    {
        await service.DeleteAsync(id, cancellationToken);
        return Results.NoContent();
    }
}
