using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Application.Services;

public interface ICreateAppService<TGetOutputDto, in TCreateInput>
{
    Task<TGetOutputDto> CreateAsync(TCreateInput input, CancellationToken cancellationToken = default);
}
