using System.Threading;

namespace ChengYuan.Core.Data;

public sealed class UnitOfWorkAccessor : IUnitOfWorkAccessor
{
    private readonly AsyncLocal<Scope?> _currentScope = new();

    public IUnitOfWork? Current => _currentScope.Value?.UnitOfWork;

    public IDisposable Change(IUnitOfWork? unitOfWork)
    {
        var previousScope = _currentScope.Value;
        var currentScope = new Scope(this, previousScope, unitOfWork);
        _currentScope.Value = currentScope;
        return currentScope;
    }

    private sealed class Scope(UnitOfWorkAccessor owner, Scope? previousScope, IUnitOfWork? unitOfWork) : IDisposable
    {
        private bool _disposed;

        public IUnitOfWork? UnitOfWork { get; } = unitOfWork;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            owner._currentScope.Value = previousScope;
            _disposed = true;
        }
    }
}
