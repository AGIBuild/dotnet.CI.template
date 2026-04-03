using ChengYuan.Core.Modularity;
using ChengYuan.Core.Timing;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.ExecutionContext;

public static class ExecutionContextServiceCollectionExtensions
{
    public static IServiceCollection AddExecutionContext(this IServiceCollection services)
    {
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<ICurrentCorrelationAccessor, CurrentCorrelationAccessor>();
        services.AddSingleton<ICurrentCorrelation>(serviceProvider => serviceProvider.GetRequiredService<ICurrentCorrelationAccessor>());
        services.AddSingleton<ICurrentUserAccessor, CurrentUserAccessor>();
        services.AddSingleton<ICurrentUser>(serviceProvider => serviceProvider.GetRequiredService<ICurrentUserAccessor>());
        return services;
    }
}

public sealed class ExecutionContextModule : ModuleBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddExecutionContext();
    }
}

internal sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

internal sealed class CurrentCorrelationAccessor : ICurrentCorrelationAccessor
{
    private readonly AsyncLocal<string?> _currentCorrelationId = new();

    public string CorrelationId
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_currentCorrelationId.Value))
                _currentCorrelationId.Value = Guid.NewGuid().ToString("N");

            return _currentCorrelationId.Value!;
        }
    }

    public IDisposable Change(string? correlationId)
    {
        var previousCorrelationId = _currentCorrelationId.Value;
        _currentCorrelationId.Value = correlationId;
        return new DelegateDisposable(() => _currentCorrelationId.Value = previousCorrelationId);
    }
}

internal sealed class CurrentUserAccessor : ICurrentUserAccessor
{
    private static readonly CurrentUserInfo AnonymousUser = new(null, null, false);
    private readonly AsyncLocal<CurrentUserInfo?> _currentUser = new();

    public string? Id => (_currentUser.Value ?? AnonymousUser).Id;

    public string? UserName => (_currentUser.Value ?? AnonymousUser).UserName;

    public bool IsAuthenticated => (_currentUser.Value ?? AnonymousUser).IsAuthenticated;

    public IDisposable Change(CurrentUserInfo? currentUser)
    {
        var previousUser = _currentUser.Value;
        _currentUser.Value = currentUser;
        return new DelegateDisposable(() => _currentUser.Value = previousUser);
    }
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
