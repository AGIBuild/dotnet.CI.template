using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ChengYuan.Core.StronglyTypedIds;

internal static class StronglyTypedIdActivatorCache<TStronglyTypedId, TValue>
    where TStronglyTypedId : StronglyTypedId<TValue>
    where TValue : notnull
{
    private static readonly Func<TValue, TStronglyTypedId> Creator = BuildCreator();

    public static TStronglyTypedId Create(TValue value) => Creator(value);

    private static Func<TValue, TStronglyTypedId> BuildCreator()
    {
        var constructor = typeof(TStronglyTypedId).GetConstructor(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            [typeof(TValue)],
            modifiers: null);

        if (constructor is null)
        {
            throw new InvalidOperationException(
                $"Strongly typed id '{typeof(TStronglyTypedId).FullName}' must declare a constructor with a single '{typeof(TValue).FullName}' parameter.");
        }

        var valueParameter = Expression.Parameter(typeof(TValue), "value");
        var constructorCall = Expression.New(constructor, valueParameter);
        return Expression.Lambda<Func<TValue, TStronglyTypedId>>(constructorCall, valueParameter).Compile();
    }
}
