namespace ChengYuan.Auditing;

public interface IAuditScopeAccessor
{
    IAuditScope? Current { get; }
}
