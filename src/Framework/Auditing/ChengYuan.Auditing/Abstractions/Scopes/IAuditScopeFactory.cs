using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Auditing;

public interface IAuditScopeFactory
{
    IAuditScope Create(string name);

    ValueTask ExecuteAsync(string name, Func<CancellationToken, ValueTask> action, CancellationToken cancellationToken = default);

    ValueTask<TResult> ExecuteAsync<TResult>(string name, Func<CancellationToken, ValueTask<TResult>> action, CancellationToken cancellationToken = default);
}
