using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Core.Modularity;

public abstract class ModuleBase
{
    public virtual void ConfigureServices(IServiceCollection services)
    {
    }
}
