using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChengYuan.Core.Timing;

namespace ChengYuan.Auditing;

internal sealed class DefaultAuditScope : IAuditScope
{
    private readonly IClock _clock;
    private readonly IAuditScopeAccessor _scopeAccessor;
    private readonly IReadOnlyCollection<IAuditLogContributor> _contributors;
    private readonly IReadOnlyCollection<IAuditLogSink> _sinks;
    private readonly IAuditScope? _previousScope;
    private bool _completed;
    private bool _disposed;

    internal DefaultAuditScope(
        AuditLogEntry entry,
        IClock clock,
        IAuditScopeAccessor scopeAccessor,
        IReadOnlyCollection<IAuditLogContributor> contributors,
        IReadOnlyCollection<IAuditLogSink> sinks)
    {
        Entry = entry;
        _clock = clock;
        _scopeAccessor = scopeAccessor;
        _contributors = contributors;
        _sinks = sinks;

        _previousScope = scopeAccessor.Current;
        ((AmbientAuditScopeAccessor)scopeAccessor).Current = this;
    }

    public AuditLogEntry Entry { get; }

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
        ((AmbientAuditScopeAccessor)_scopeAccessor).Current = _previousScope;
        await CompleteAsync(CancellationToken.None);
    }

    private async ValueTask CompleteAsync(CancellationToken cancellationToken)
    {
        if (_completed)
        {
            return;
        }

        _completed = true;
        var completedAtUtc = _clock.UtcNow;
        Entry.CompletedAtUtc = completedAtUtc;
        Entry.Duration = completedAtUtc - Entry.StartedAtUtc;

        foreach (var contributor in _contributors)
        {
            await contributor.ContributeAsync(Entry, cancellationToken);
        }

        foreach (var sink in _sinks)
        {
            await sink.WriteAsync(Entry, cancellationToken);
        }
    }
}
