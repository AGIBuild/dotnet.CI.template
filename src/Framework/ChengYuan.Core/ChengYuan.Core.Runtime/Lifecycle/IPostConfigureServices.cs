using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Core.Lifecycle;

public interface IPostConfigureServices
{
    void PostConfigureServices(IServiceCollection services);
}
