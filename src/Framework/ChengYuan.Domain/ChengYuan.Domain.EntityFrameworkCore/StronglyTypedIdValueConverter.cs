namespace ChengYuan.Domain;

public class StronglyTypedIdValueConverter<TStronglyTypedId, TValue> : ChengYuan.Core.EntityFrameworkCore.StronglyTypedIdValueConverter<TStronglyTypedId, TValue>
    where TStronglyTypedId : ChengYuan.Core.StronglyTypedIds.StronglyTypedId<TValue>
    where TValue : notnull
{
}
