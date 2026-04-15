using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Application.Services;

public interface IUpdateAppService<TGetOutputDto, in TKey, in TUpdateInput>
{
    Task<TGetOutputDto> UpdateAsync(TKey id, TUpdateInput input, CancellationToken cancellationToken = default);
}
