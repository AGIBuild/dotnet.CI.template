namespace ChengYuan.Core.DependencyInjection;

public sealed class ObjectAccessor<T>(T? value = default) : IObjectAccessor<T>
{
    public T? Value { get; set; } = value;
}
