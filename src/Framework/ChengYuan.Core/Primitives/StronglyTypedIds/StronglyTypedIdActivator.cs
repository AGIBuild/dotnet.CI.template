using System;

namespace ChengYuan.Core.StronglyTypedIds;

public static class StronglyTypedIdActivator
{
    public static TStronglyTypedId Create<TStronglyTypedId, TValue>(TValue value)
        where TStronglyTypedId : StronglyTypedId<TValue>
        where TValue : notnull
    {
        ArgumentNullException.ThrowIfNull(value);
        return StronglyTypedIdActivatorCache<TStronglyTypedId, TValue>.Create(value);
    }
}
