using System;
using ChengYuan.Core.Application;
using ChengYuan.ExecutionContext;
using ChengYuan.MultiTenancy;
using ChengYuan.ObjectMapping;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Application;

public abstract class ApplicationService : ApplicationServiceBase
{
    private ICurrentUser? _currentUser;
    private ICurrentTenant? _currentTenant;
    private IObjectMapper? _objectMapper;

    protected ApplicationService(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    protected ICurrentUser CurrentUser => _currentUser ??= ServiceProvider.GetRequiredService<ICurrentUser>();

    protected ICurrentTenant CurrentTenant => _currentTenant ??= ServiceProvider.GetRequiredService<ICurrentTenant>();

    protected IObjectMapper ObjectMapper => _objectMapper ??= ServiceProvider.GetRequiredService<IObjectMapper>();
}
