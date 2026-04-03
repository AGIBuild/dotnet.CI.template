namespace ChengYuan.Core.Data;

public interface IDataTenantProvider
{
    Guid? TenantId { get; }

    bool IsAvailable { get; }
}

public sealed class NullDataTenantProvider : IDataTenantProvider
{
    public Guid? TenantId => null;

    public bool IsAvailable => false;
}
