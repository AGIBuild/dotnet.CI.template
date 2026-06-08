using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Core.Data;

public sealed class UnitOfWorkManager(
    IServiceProvider serviceProvider,
    IUnitOfWorkAccessor accessor) : IUnitOfWorkManager
{
    public IUnitOfWork? Current => accessor.Current;

    public IUnitOfWork Begin(UnitOfWorkOptions? options = null)
    {
        if (accessor.Current is { } current)
        {
            return new ChildUnitOfWork(current);
        }

        var innerUnitOfWork = serviceProvider.GetService<IUnitOfWork>() ?? new NullUnitOfWork();
        var effectiveOptions = options ?? UnitOfWorkOptions.Default;

        if (innerUnitOfWork is IUnitOfWorkInitializer initializer)
        {
            initializer.Initialize(effectiveOptions);
        }

        var unitOfWork = new ManagedUnitOfWork(innerUnitOfWork, effectiveOptions);
        unitOfWork.AttachAmbientScope(accessor.Change(unitOfWork));
        return unitOfWork;
    }

    private sealed class ManagedUnitOfWork(IUnitOfWork innerUnitOfWork, UnitOfWorkOptions options) : IUnitOfWork
    {
        private IDisposable? _ambientScope;
        private bool _disposed;

        public UnitOfWorkOptions Options => options;

        public bool IsCompleted { get; private set; }

        public bool IsRolledBack { get; private set; }

        public void AttachAmbientScope(IDisposable ambientScope)
        {
            _ambientScope = ambientScope;
        }

        public ValueTask SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            return innerUnitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async ValueTask CompleteAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            if (IsCompleted)
            {
                return;
            }

            if (IsRolledBack)
            {
                throw new InvalidOperationException("The unit of work has already been rolled back.");
            }

            await innerUnitOfWork.CompleteAsync(cancellationToken);
            IsCompleted = true;
        }

        public async ValueTask RollbackAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            if (IsCompleted || IsRolledBack)
            {
                return;
            }

            await innerUnitOfWork.RollbackAsync(cancellationToken);
            IsRolledBack = true;
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
            {
                return;
            }

            if (!IsCompleted && !IsRolledBack)
            {
                await innerUnitOfWork.RollbackAsync();
                IsRolledBack = true;
            }

            await innerUnitOfWork.DisposeAsync();
            _ambientScope?.Dispose();
            _disposed = true;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            if (!IsCompleted && !IsRolledBack)
            {
                innerUnitOfWork.RollbackAsync().AsTask().GetAwaiter().GetResult();
                IsRolledBack = true;
            }

            innerUnitOfWork.Dispose();
            _ambientScope?.Dispose();
            _disposed = true;
        }

        private void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
        }
    }

    private sealed class ChildUnitOfWork(IUnitOfWork parent) : IUnitOfWork
    {
        public UnitOfWorkOptions Options => parent.Options;

        public bool IsCompleted { get; private set; }

        public bool IsRolledBack { get; private set; }

        public ValueTask SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return parent.SaveChangesAsync(cancellationToken);
        }

        public ValueTask CompleteAsync(CancellationToken cancellationToken = default)
        {
            IsCompleted = true;
            return ValueTask.CompletedTask;
        }

        public ValueTask RollbackAsync(CancellationToken cancellationToken = default)
        {
            IsRolledBack = true;
            return parent.RollbackAsync(cancellationToken);
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

        public void Dispose()
        {
        }
    }

}
