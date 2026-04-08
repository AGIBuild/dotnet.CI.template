using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Core.Data;

public interface IUnitOfWork
{
    ValueTask SaveChangesAsync(CancellationToken cancellationToken = default);
}
