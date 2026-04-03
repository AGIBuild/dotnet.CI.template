using System.Threading;

namespace ChengYuan.Core.Data;

public interface IDataFilter<TFilter>
{
    bool IsEnabled { get; }

    IDisposable Enable();

    IDisposable Disable();
}

public sealed class DataFilter<TFilter> : IDataFilter<TFilter>
{
    private readonly AsyncLocal<FilterScope?> _currentScope = new();

    public bool IsEnabled => _currentScope.Value?.IsEnabled ?? true;

    public IDisposable Enable()
    {
        return Change(true);
    }

    public IDisposable Disable()
    {
        return Change(false);
    }

    private FilterScope Change(bool isEnabled)
    {
        var previousScope = _currentScope.Value;
        var currentScope = new FilterScope(this, previousScope, isEnabled);
        _currentScope.Value = currentScope;
        return currentScope;
    }

    private sealed class FilterScope(DataFilter<TFilter> owner, FilterScope? previousScope, bool isEnabled) : IDisposable
    {
        private bool _disposed;

        public bool IsEnabled { get; } = isEnabled;

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

public sealed class SoftDeleteFilter
{
}

public sealed class MultiTenantFilter
{
}
