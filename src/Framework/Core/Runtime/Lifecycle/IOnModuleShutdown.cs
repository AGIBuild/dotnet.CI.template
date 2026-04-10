namespace ChengYuan.Core.Lifecycle;

public interface IOnModuleShutdown
{
    Task ShutdownAsync(IModuleShutdownContext context);
}
