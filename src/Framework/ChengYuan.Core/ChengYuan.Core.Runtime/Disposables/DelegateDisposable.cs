using System;

namespace ChengYuan.Core;

public sealed class DelegateDisposable(Action onDispose) : IDisposable
{
    private bool _disposed;

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        onDispose();
    }
}
