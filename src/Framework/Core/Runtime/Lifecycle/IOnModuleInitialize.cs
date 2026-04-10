namespace ChengYuan.Core.Lifecycle;

public interface IOnModuleInitialize
{
    Task InitializeAsync(IModuleInitializationContext context);
}
