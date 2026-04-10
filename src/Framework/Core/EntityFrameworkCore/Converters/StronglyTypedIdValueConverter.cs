using System.Linq.Expressions;
using ChengYuan.Core.StronglyTypedIds;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ChengYuan.Core.EntityFrameworkCore;

public class StronglyTypedIdValueConverter<TStronglyTypedId, TValue> : ValueConverter<TStronglyTypedId, TValue>
    where TStronglyTypedId : StronglyTypedId<TValue>
    where TValue : notnull
{
    public StronglyTypedIdValueConverter()
        : base(ToProviderExpression, FromProviderExpression)
    {
    }

    private static Expression<Func<TStronglyTypedId, TValue>> ToProviderExpression => stronglyTypedId => stronglyTypedId.Value;

    private static Expression<Func<TValue, TStronglyTypedId>> FromProviderExpression => value => StronglyTypedIdActivator.Create<TStronglyTypedId, TValue>(value);
}
