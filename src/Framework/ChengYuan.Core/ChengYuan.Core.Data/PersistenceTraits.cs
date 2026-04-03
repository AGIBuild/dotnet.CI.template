namespace ChengYuan.Core.Data;

public interface ISoftDelete
{
    bool IsDeleted { get; }
}

public interface IMultiTenant
{
    Guid? TenantId { get; }
}

public interface IHasCreationTime
{
    DateTimeOffset CreationTime { get; }
}

public interface IHasModificationTime
{
    DateTimeOffset? LastModificationTime { get; }
}

public interface IHasDeletionTime
{
    DateTimeOffset? DeletionTime { get; }
}
