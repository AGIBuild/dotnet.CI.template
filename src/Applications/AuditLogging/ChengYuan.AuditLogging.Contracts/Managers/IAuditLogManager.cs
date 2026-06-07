namespace ChengYuan.AuditLogging;

public interface IAuditLogManager
{
    ValueTask<IReadOnlyList<AuditLogRecord>> GetListAsync(CancellationToken cancellationToken = default);
}
