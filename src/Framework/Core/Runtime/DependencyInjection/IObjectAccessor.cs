namespace ChengYuan.Core.DependencyInjection;

public interface IObjectAccessor<out T>
{
    T? Value { get; }
}
