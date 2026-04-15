using ChengYuan.Core.Application.Dtos;

namespace ChengYuan.Application.Services;

public interface ICrudAppService<TGetOutputDto, TGetListOutputDto, in TKey, in TGetListInput, in TCreateInput, in TUpdateInput>
    : IReadOnlyAppService<TGetOutputDto, TGetListOutputDto, TKey, TGetListInput>,
      ICreateAppService<TGetOutputDto, TCreateInput>,
      IUpdateAppService<TGetOutputDto, TKey, TUpdateInput>,
      IDeleteAppService<TKey>;

public interface ICrudAppService<TEntityDto, in TKey, in TGetListInput, in TCreateInput, in TUpdateInput>
    : ICrudAppService<TEntityDto, TEntityDto, TKey, TGetListInput, TCreateInput, TUpdateInput>;

public interface ICrudAppService<TEntityDto, in TKey>
    : ICrudAppService<TEntityDto, TKey, PagedAndSortedResultRequestDto, TEntityDto, TEntityDto>;
