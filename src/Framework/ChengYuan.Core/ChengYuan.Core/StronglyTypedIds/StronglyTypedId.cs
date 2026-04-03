using System.Linq.Expressions;
using System.Reflection;

namespace ChengYuan.Core.StronglyTypedIds;

public interface IStronglyTypedId<out TValue>
{
    TValue Value { get; }
}

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

public abstract record GuidStronglyTypedId : StronglyTypedId<Guid>
{
    protected GuidStronglyTypedId(Guid value)
        : base(value == Guid.Empty
            ? throw new ArgumentException("Strongly typed id values cannot be empty.", nameof(value))
            : value)
    {
    }
}

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
