using System;
using ChengYuan.Core;

namespace ChengYuan.Core.StronglyTypedIds;

public abstract record GuidStronglyTypedId : StronglyTypedId<Guid>
{
    protected GuidStronglyTypedId(Guid value)
        : base(Check.NotEmpty(value, nameof(value)))
    {
    }
}
