namespace ChengYuan.Core.Modularity;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class DependsOnAttribute(params Type[] moduleTypes) : Attribute
{
    public IReadOnlyList<Type> ModuleTypes { get; } = moduleTypes;
}
