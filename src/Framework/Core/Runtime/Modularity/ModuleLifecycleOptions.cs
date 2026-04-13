namespace ChengYuan.Core.Modularity;

public sealed class ModuleLifecycleOptions
{
    public IList<Type> Contributors { get; } = new List<Type>();
}
