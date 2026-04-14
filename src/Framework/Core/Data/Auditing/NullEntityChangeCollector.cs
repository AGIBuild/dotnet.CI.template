namespace ChengYuan.Core.Data.Auditing;

public sealed class NullEntityChangeCollector : IEntityChangeCollector
{
    public static NullEntityChangeCollector Instance { get; } = new();

    public bool IsActive => false;

    public void Collect(EntityChangeInfo changeInfo)
    {
    }
}
