using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChengYuan.Core.Timing;

namespace ChengYuan.Auditing;

internal sealed class DefaultAuditScope(
    AuditLogEntry entry,
    IClock clock,
    IReadOnlyCollection<IAuditLogContributor> contributors,
    IReadOnlyCollection<IAuditLogSink> sinks) : IAuditScope
{
    private bool _completed;
    private bool _disposed;

    public AuditLogEntry Entry { get; } = entry;

    public void SetProperty(string name, object? value)
    {
        Entry.SetProperty(name, value);
    }

    public void MarkSucceeded()
    {
        Entry.Succeeded = true;
        Entry.ErrorMessage = null;
    }

    public void MarkFailed(string? errorMessage = null)
    {
        Entry.Succeeded = false;
        Entry.ErrorMessage = string.IsNullOrWhiteSpace(errorMessage) ? Entry.ErrorMessage : errorMessage;
    }

    public void MarkFailed(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        Entry.Succeeded = false;
        Entry.ErrorMessage = exception.Message;
        Entry.SetProperty("exceptionType", exception.GetType().FullName);
    }

    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        await CompleteAsync(CancellationToken.None);
    }

    private async ValueTask CompleteAsync(CancellationToken cancellationToken)
    {
        if (_completed)
        {
            return;
        }

        _completed = true;
        var completedAtUtc = clock.UtcNow;
        Entry.CompletedAtUtc = completedAtUtc;
        Entry.Duration = completedAtUtc - Entry.StartedAtUtc;

        foreach (var contributor in contributors)
        {
            await contributor.ContributeAsync(Entry, cancellationToken);
        }

        foreach (var sink in sinks)
        {
            await sink.WriteAsync(Entry, cancellationToken);
        }
    }
}
