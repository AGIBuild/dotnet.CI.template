using System;

namespace ChengYuan.ExecutionContext;

public interface ICurrentCorrelationAccessor : ICurrentCorrelation
{
    IDisposable Change(string? correlationId);
}
