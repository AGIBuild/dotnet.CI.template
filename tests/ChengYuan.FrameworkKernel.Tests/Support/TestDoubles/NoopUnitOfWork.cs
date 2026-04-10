using ChengYuan.Core.Data;

namespace ChengYuan.FrameworkKernel.Tests;

internal sealed class NoopUnitOfWork : IUnitOfWork
{
    public ValueTask SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return ValueTask.CompletedTask;
    }
}
