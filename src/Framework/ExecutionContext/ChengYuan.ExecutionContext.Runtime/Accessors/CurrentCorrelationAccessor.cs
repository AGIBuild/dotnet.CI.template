using System;
using System.Threading;
using ChengYuan.Core;

namespace ChengYuan.ExecutionContext;

internal sealed class CurrentCorrelationAccessor : ICurrentCorrelationAccessor
{
    private readonly AsyncLocal<string?> _currentCorrelationId = new();

    public string CorrelationId
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_currentCorrelationId.Value))
                _currentCorrelationId.Value = Guid.NewGuid().ToString("N");

            return _currentCorrelationId.Value!;
        }
    }

    public IDisposable Change(string? correlationId)
    {
        var previousCorrelationId = _currentCorrelationId.Value;
        _currentCorrelationId.Value = correlationId;
        return new DelegateDisposable(() => _currentCorrelationId.Value = previousCorrelationId);
    }
}
