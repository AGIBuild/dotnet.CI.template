using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChengYuan.Core.Timing;
using ChengYuan.ExecutionContext;
using ChengYuan.MultiTenancy;

namespace ChengYuan.Auditing;

internal sealed class DefaultAuditScopeFactory(
    IClock clock,
    ICurrentTenant currentTenant,
    ICurrentUser currentUser,
    ICurrentCorrelation currentCorrelation,
    IAuditScopeAccessor scopeAccessor,
    IEnumerable<IAuditLogContributor> contributors,
    IEnumerable<IAuditLogSink> sinks) : IAuditScopeFactory
{
    private readonly IAuditLogContributor[] _contributors = contributors.ToArray();
    private readonly IAuditLogSink[] _sinks = sinks.ToArray();

    public IAuditScope Create(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var entry = new AuditLogEntry(name, clock.UtcNow)
        {
            TenantId = currentTenant.Id,
            UserId = currentUser.Id,
            UserName = currentUser.UserName,
            IsAuthenticated = currentUser.IsAuthenticated,
            CorrelationId = currentCorrelation.CorrelationId
        };

        return new DefaultAuditScope(entry, clock, scopeAccessor, _contributors, _sinks);
    }

    public async ValueTask ExecuteAsync(string name, Func<CancellationToken, ValueTask> action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        await using var scope = Create(name);

        try
        {
            await action(cancellationToken);
            scope.MarkSucceeded();
        }
        catch (Exception exception)
        {
            scope.MarkFailed(exception);
            throw;
        }
    }

    public async ValueTask<TResult> ExecuteAsync<TResult>(string name, Func<CancellationToken, ValueTask<TResult>> action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        await using var scope = Create(name);

        try
        {
            var result = await action(cancellationToken);
            scope.MarkSucceeded();
            return result;
        }
        catch (Exception exception)
        {
            scope.MarkFailed(exception);
            throw;
        }
    }
}
