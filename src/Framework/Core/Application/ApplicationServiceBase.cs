using System;
using ChengYuan.Core.Guids;
using ChengYuan.Core.Timing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChengYuan.Core.Application;

public abstract class ApplicationServiceBase : IApplicationService
{
    private IClock? _clock;
    private IGuidGenerator? _guidGenerator;
    private ILogger? _logger;

    protected ApplicationServiceBase(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ServiceProvider = serviceProvider;
    }

    protected IServiceProvider ServiceProvider { get; }

    protected IClock Clock => _clock ??= ServiceProvider.GetRequiredService<IClock>();

    protected IGuidGenerator GuidGenerator => _guidGenerator ??= ServiceProvider.GetRequiredService<IGuidGenerator>();

    protected ILogger Logger => _logger ??= ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(GetType());
}
