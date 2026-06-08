namespace ChengYuan.Core.Data;

internal sealed class NullUnitOfWork : IUnitOfWork
{
    public UnitOfWorkOptions Options => UnitOfWorkOptions.Default;

    public bool IsCompleted { get; private set; }

    public bool IsRolledBack { get; private set; }

    public ValueTask SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask CompleteAsync(CancellationToken cancellationToken = default)
    {
        IsCompleted = true;
        return ValueTask.CompletedTask;
    }

    public ValueTask RollbackAsync(CancellationToken cancellationToken = default)
    {
        IsRolledBack = true;
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
    }
}
