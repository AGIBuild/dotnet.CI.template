using System;

namespace ChengYuan.Core.StronglyTypedIds;

public abstract record StronglyTypedId<TValue> : IStronglyTypedId<TValue>
    where TValue : notnull
{
    protected StronglyTypedId(TValue value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Value = value;
    }

    public TValue Value { get; }

    public string ValueText => Value.ToString() ?? string.Empty;
}
