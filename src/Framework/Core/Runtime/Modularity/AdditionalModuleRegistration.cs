namespace ChengYuan.Core.Modularity;

internal sealed class AdditionalModuleRegistration(Type moduleType)
{
    public Type ModuleType { get; } = moduleType;
}
