using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Core.Lifecycle;

public interface IPreConfigureServices
{
    void PreConfigureServices(IServiceCollection services);
}
