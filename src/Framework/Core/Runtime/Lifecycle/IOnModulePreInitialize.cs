namespace ChengYuan.Core.Lifecycle;

public interface IOnModulePreInitialize
{
    Task PreInitializeAsync(IModuleInitializationContext context);
}
