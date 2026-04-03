using ChengYuan.Core.Data;
using ChengYuan.Core.Modularity;
using ChengYuan.ExecutionContext;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.MultiTenancy;

public static class MultiTenancyServiceCollectionExtensions
{
    public static IServiceCollection AddMultiTenancy(this IServiceCollection services)
    {
        services.AddSingleton<ICurrentTenantAccessor, CurrentTenantAccessor>();
        services.AddSingleton<ICurrentTenant>(serviceProvider => serviceProvider.GetRequiredService<ICurrentTenantAccessor>());
        services.Replace(ServiceDescriptor.Singleton<IDataTenantProvider>(
            serviceProvider => new CurrentTenantDataTenantProvider(serviceProvider.GetRequiredService<ICurrentTenant>())));
        return services;
    }
}

[DependsOn(typeof(ExecutionContextModule))]
public sealed class MultiTenancyModule : ModuleBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddMultiTenancy();
    }
}

internal sealed class CurrentTenantAccessor : ICurrentTenantAccessor
{
    private readonly AsyncLocal<TenantInfo?> _currentTenant = new();

    public Guid? Id => _currentTenant.Value?.Id;

    public string? Name => _currentTenant.Value?.Name;

    public bool IsAvailable => Id.HasValue;

    public IDisposable Change(Guid? tenantId, string? tenantName = null)
    {
        var previousTenant = _currentTenant.Value;
        _currentTenant.Value = new TenantInfo(tenantId, tenantName);
        return new DelegateDisposable(() => _currentTenant.Value = previousTenant);
    }
}

internal sealed class CurrentTenantDataTenantProvider(ICurrentTenant currentTenant) : IDataTenantProvider
{
    public Guid? TenantId => currentTenant.Id;

    public bool IsAvailable => currentTenant.IsAvailable;
}

internal sealed class DelegateDisposable(Action onDispose) : IDisposable
{
    private bool _disposed;

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        onDispose();
    }
}
