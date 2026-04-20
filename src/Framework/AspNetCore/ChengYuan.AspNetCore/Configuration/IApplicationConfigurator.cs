using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.AspNetCore.Configuration;

public interface IApplicationConfigurator
{
    Task<ApplicationConfigurationDto> BuildAsync(CancellationToken cancellationToken = default);
}
