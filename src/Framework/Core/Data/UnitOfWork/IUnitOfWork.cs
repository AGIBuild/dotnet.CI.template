using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Core.Data;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    UnitOfWorkOptions Options { get; }

    bool IsCompleted { get; }

    bool IsRolledBack { get; }

    ValueTask SaveChangesAsync(CancellationToken cancellationToken = default);

    ValueTask CompleteAsync(CancellationToken cancellationToken = default);

    ValueTask RollbackAsync(CancellationToken cancellationToken = default);
}
