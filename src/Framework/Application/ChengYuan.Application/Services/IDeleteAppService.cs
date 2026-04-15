using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Application.Services;

public interface IDeleteAppService<in TKey>
{
    Task DeleteAsync(TKey id, CancellationToken cancellationToken = default);
}
