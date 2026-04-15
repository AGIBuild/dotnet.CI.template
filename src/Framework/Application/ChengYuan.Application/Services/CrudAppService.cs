using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChengYuan.Core.Application.Dtos;
using ChengYuan.Core.Data;
using ChengYuan.Core.Entities;

namespace ChengYuan.Application.Services;

public abstract class CrudAppService<TEntity, TGetOutputDto, TGetListOutputDto, TKey, TGetListInput, TCreateInput, TUpdateInput>(
    IServiceProvider serviceProvider,
    IRepository<TEntity, TKey> repository)
    : ApplicationService(serviceProvider),
      ICrudAppService<TGetOutputDto, TGetListOutputDto, TKey, TGetListInput, TCreateInput, TUpdateInput>
    where TEntity : class, IAggregateRoot<TKey>
    where TKey : notnull
{
    protected IRepository<TEntity, TKey> Repository { get; } = repository;

    public virtual async Task<TGetOutputDto> GetAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken);
        return MapToGetOutputDto(entity);
    }

    public virtual async Task<PagedResultDto<TGetListOutputDto>> GetListAsync(TGetListInput input, CancellationToken cancellationToken = default)
    {
        var totalCount = await Repository.GetCountAsync(cancellationToken);

        var (skipCount, maxResultCount, sorting) = ExtractPaging(input);
        var entities = await Repository.GetPagedListAsync(skipCount, maxResultCount, sorting, cancellationToken);

        var dtos = MapToGetListOutputDtos(entities);
        return new PagedResultDto<TGetListOutputDto>(totalCount, dtos);
    }

    public virtual async Task<TGetOutputDto> CreateAsync(TCreateInput input, CancellationToken cancellationToken = default)
    {
        var entity = MapToEntity(input);
        await Repository.InsertAsync(entity, cancellationToken);
        await SaveChangesAsync(cancellationToken);
        return MapToGetOutputDto(entity);
    }

    public virtual async Task<TGetOutputDto> UpdateAsync(TKey id, TUpdateInput input, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken);
        MapToEntity(input, entity);
        await SaveChangesAsync(cancellationToken);
        return MapToGetOutputDto(entity);
    }

    public virtual async Task DeleteAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken);
        await Repository.DeleteAsync(entity, cancellationToken);
        await SaveChangesAsync(cancellationToken);
    }

    protected virtual TGetOutputDto MapToGetOutputDto(TEntity entity)
    {
        return ObjectMapper.Map<TEntity, TGetOutputDto>(entity);
    }

    protected virtual TGetListOutputDto MapToGetListOutputDto(TEntity entity)
    {
        return ObjectMapper.Map<TEntity, TGetListOutputDto>(entity);
    }

    protected virtual List<TGetListOutputDto> MapToGetListOutputDtos(List<TEntity> entities)
    {
        return entities.ConvertAll(MapToGetListOutputDto);
    }

    protected virtual TEntity MapToEntity(TCreateInput input)
    {
        return ObjectMapper.Map<TCreateInput, TEntity>(input);
    }

    protected virtual void MapToEntity(TUpdateInput input, TEntity entity)
    {
        ObjectMapper.Map(input, entity);
    }

    protected virtual (int SkipCount, int MaxResultCount, string? Sorting) ExtractPaging(TGetListInput input)
    {
        if (input is PagedAndSortedResultRequestDto paged)
        {
            return (paged.SkipCount, paged.MaxResultCount, paged.Sorting);
        }

        return (0, PagedAndSortedResultRequestDto.DefaultMaxResultCount, null);
    }

    protected virtual async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (CurrentUnitOfWork is not null)
        {
            await CurrentUnitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}

public abstract class CrudAppService<TEntity, TEntityDto, TKey, TGetListInput, TCreateInput, TUpdateInput>(
    IServiceProvider serviceProvider,
    IRepository<TEntity, TKey> repository)
    : CrudAppService<TEntity, TEntityDto, TEntityDto, TKey, TGetListInput, TCreateInput, TUpdateInput>(serviceProvider, repository)
    where TEntity : class, IAggregateRoot<TKey>
    where TKey : notnull;

public abstract class CrudAppService<TEntity, TEntityDto, TKey>(
    IServiceProvider serviceProvider,
    IRepository<TEntity, TKey> repository)
    : CrudAppService<TEntity, TEntityDto, TKey, PagedAndSortedResultRequestDto, TEntityDto, TEntityDto>(serviceProvider, repository)
    where TEntity : class, IAggregateRoot<TKey>
    where TKey : notnull;
