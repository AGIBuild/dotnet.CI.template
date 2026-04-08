using System;

namespace ChengYuan.Auditing;

public interface IAuditScope : IDisposable, IAsyncDisposable
{
    AuditLogEntry Entry { get; }

    void SetProperty(string name, object? value);

    void MarkSucceeded();

    void MarkFailed(string? errorMessage = null);

    void MarkFailed(Exception exception);
}
