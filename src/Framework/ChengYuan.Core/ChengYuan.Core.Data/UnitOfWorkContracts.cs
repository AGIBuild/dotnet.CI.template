namespace ChengYuan.Core.Data;

public interface IUnitOfWork
{
    ValueTask SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IUnitOfWorkAccessor
{
    IUnitOfWork? Current { get; }

    IDisposable Change(IUnitOfWork? unitOfWork);
}
