using System.Threading;
using System.Threading.Tasks;
using ChengYuan.Core.Application.Dtos;

namespace ChengYuan.Application.Services;

public interface IReadOnlyAppService<TGetOutputDto, TGetListOutputDto, in TKey, in TGetListInput>
{
    Task<TGetOutputDto> GetAsync(TKey id, CancellationToken cancellationToken = default);

    Task<PagedResultDto<TGetListOutputDto>> GetListAsync(TGetListInput input, CancellationToken cancellationToken = default);
}
