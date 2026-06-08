namespace ChengYuan.Core.Data;

public sealed class UnitOfWorkOptions
{
    public static UnitOfWorkOptions Default { get; } = new();

    public UnitOfWorkTransactionBehavior TransactionBehavior { get; init; } = UnitOfWorkTransactionBehavior.Auto;

    public bool IsTransactional => TransactionBehavior == UnitOfWorkTransactionBehavior.Enabled;
}
