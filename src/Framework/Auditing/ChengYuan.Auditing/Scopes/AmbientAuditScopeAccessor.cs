namespace ChengYuan.Auditing;

internal sealed class AmbientAuditScopeAccessor : IAuditScopeAccessor
{
    private static readonly AsyncLocal<IAuditScope?> Holder = new();

    public IAuditScope? Current
    {
        get => Holder.Value;
        set => Holder.Value = value;
    }
}
