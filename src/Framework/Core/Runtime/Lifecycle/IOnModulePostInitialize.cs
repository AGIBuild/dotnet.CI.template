namespace ChengYuan.Core.Lifecycle;

public interface IOnModulePostInitialize
{
    Task PostInitializeAsync(IModuleInitializationContext context);
}
