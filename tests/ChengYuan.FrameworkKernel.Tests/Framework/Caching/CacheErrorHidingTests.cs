using ChengYuan.Caching;
using ChengYuan.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public sealed class CacheErrorHidingTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    public CacheErrorHidingTests()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ICurrentTenant, NullCurrentTenant>();
        services.AddCaching();
        services.AddSingleton<IChengYuanCacheStore, FailingCacheStore>();
        services.Configure<ChengYuanCacheOptions>(o => o.HideErrors = true);
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task GetAsync_ReturnsNull_WhenStoreFailsAndHideErrorsEnabled()
    {
        var cache = _serviceProvider.GetRequiredService<IChengYuanCache<TestCacheItem>>();

        var result = await cache.GetAsync("fail", TestContext.Current.CancellationToken);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task SetAsync_DoesNotThrow_WhenStoreFailsAndHideErrorsEnabled()
    {
        var cache = _serviceProvider.GetRequiredService<IChengYuanCache<TestCacheItem>>();

        await Should.NotThrowAsync(() => cache.SetAsync("fail", new TestCacheItem("v"), cancellationToken: TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task ExistsAsync_ReturnsFalse_WhenStoreFailsAndHideErrorsEnabled()
    {
        var cache = _serviceProvider.GetRequiredService<IChengYuanCache<TestCacheItem>>();

        var exists = await cache.ExistsAsync("fail", TestContext.Current.CancellationToken);

        exists.ShouldBeFalse();
    }

    [Fact]
    public async Task RemoveAsync_DoesNotThrow_WhenStoreFailsAndHideErrorsEnabled()
    {
        var cache = _serviceProvider.GetRequiredService<IChengYuanCache<TestCacheItem>>();

        await Should.NotThrowAsync(() => cache.RemoveAsync("fail", TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task GetAsync_Throws_WhenStoreFailsAndHideErrorsDisabled()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ICurrentTenant, NullCurrentTenant>();
        services.AddCaching();
        services.AddSingleton<IChengYuanCacheStore, FailingCacheStore>();
        services.Configure<ChengYuanCacheOptions>(o => o.HideErrors = false);
        using var sp = services.BuildServiceProvider();
        var cache = sp.GetRequiredService<IChengYuanCache<TestCacheItem>>();

        await Should.ThrowAsync<InvalidOperationException>(() => cache.GetAsync("fail", TestContext.Current.CancellationToken).AsTask());
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
        GC.SuppressFinalize(this);
    }
}

internal sealed class FailingCacheStore : IChengYuanCacheStore
{
    public ValueTask<ChengYuanCacheItem?> GetAsync(string normalizedKey, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("Store failed");

    public ValueTask SetAsync(string normalizedKey, ChengYuanCacheItem item, ChengYuanCacheEntryOptions options, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("Store failed");

    public ValueTask RemoveAsync(string normalizedKey, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("Store failed");
}
