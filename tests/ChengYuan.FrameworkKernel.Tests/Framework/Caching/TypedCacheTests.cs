using ChengYuan.Caching;
using ChengYuan.MultiTenancy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public sealed class TypedCacheTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IChengYuanCache<TestCacheItem> _cache;

    public TypedCacheTests()
    {
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddSingleton<ICurrentTenant, NullCurrentTenant>();
        services.AddCaching();
        services.AddSingleton<IChengYuanCacheStore, TestMemoryCacheStore>();
        services.Configure<ChengYuanCacheOptions>(_ => { });
        _serviceProvider = services.BuildServiceProvider();
        _cache = _serviceProvider.GetRequiredService<IChengYuanCache<TestCacheItem>>();
    }

    [Fact]
    public async Task GetAsync_ReturnsNull_WhenKeyDoesNotExist()
    {
        var result = await _cache.GetAsync("nonexistent", TestContext.Current.CancellationToken);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task SetAsync_ThenGetAsync_ReturnsValue()
    {
        var item = new TestCacheItem("hello");

        await _cache.SetAsync("key1", item, cancellationToken: TestContext.Current.CancellationToken);
        var result = await _cache.GetAsync("key1", TestContext.Current.CancellationToken);

        result.ShouldNotBeNull();
        result.Value.ShouldBe("hello");
    }

    [Fact]
    public async Task ExistsAsync_ReturnsTrueForExistingKey()
    {
        await _cache.SetAsync("key2", new TestCacheItem("val"), cancellationToken: TestContext.Current.CancellationToken);

        var exists = await _cache.ExistsAsync("key2", TestContext.Current.CancellationToken);

        exists.ShouldBeTrue();
    }

    [Fact]
    public async Task ExistsAsync_ReturnsFalseForMissingKey()
    {
        var exists = await _cache.ExistsAsync("missing", TestContext.Current.CancellationToken);

        exists.ShouldBeFalse();
    }

    [Fact]
    public async Task RemoveAsync_RemovesItem()
    {
        await _cache.SetAsync("key3", new TestCacheItem("val"), cancellationToken: TestContext.Current.CancellationToken);

        await _cache.RemoveAsync("key3", TestContext.Current.CancellationToken);

        var result = await _cache.GetAsync("key3", TestContext.Current.CancellationToken);
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetOrCreateAsync_CreatesWhenMissing()
    {
        var factoryInvoked = false;

        var result = await _cache.GetOrCreateAsync("key4", _ =>
        {
            factoryInvoked = true;
            return ValueTask.FromResult(new TestCacheItem("created"));
        }, cancellationToken: TestContext.Current.CancellationToken);

        factoryInvoked.ShouldBeTrue();
        result.Value.ShouldBe("created");
    }

    [Fact]
    public async Task GetOrCreateAsync_ReturnsExistingWithoutCallingFactory()
    {
        await _cache.SetAsync("key5", new TestCacheItem("existing"), cancellationToken: TestContext.Current.CancellationToken);
        var factoryInvoked = false;

        var result = await _cache.GetOrCreateAsync("key5", _ =>
        {
            factoryInvoked = true;
            return ValueTask.FromResult(new TestCacheItem("should-not-be-used"));
        }, cancellationToken: TestContext.Current.CancellationToken);

        factoryInvoked.ShouldBeFalse();
        result.Value.ShouldBe("existing");
    }

    [Fact]
    public async Task GetOrCreateAsync_StampedeProtection_FactoryCalledOnlyOnce()
    {
        var factoryCallCount = 0;
        var ct = TestContext.Current.CancellationToken;

        var tasks = Enumerable.Range(0, 10).Select(_ =>
            _cache.GetOrCreateAsync("stampede-key", async c =>
            {
                Interlocked.Increment(ref factoryCallCount);
                await Task.Delay(50, c);
                return new TestCacheItem("value");
            }, cancellationToken: ct).AsTask()
        ).ToArray();

        await Task.WhenAll(tasks);

        factoryCallCount.ShouldBe(1);
        foreach (var task in tasks)
        {
            (await task).Value.ShouldBe("value");
        }
    }

    [Fact]
    public void DifferentCacheItemTypes_GetSeparateInstances()
    {
        var cache1 = _serviceProvider.GetRequiredService<IChengYuanCache<TestCacheItem>>();
        var cache2 = _serviceProvider.GetRequiredService<IChengYuanCache<AnotherCacheItem>>();

        cache1.ShouldNotBeSameAs(cache2);
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
        GC.SuppressFinalize(this);
    }
}

public class TestCacheItem
{
    public string Value { get; set; } = string.Empty;

    public TestCacheItem()
    {
    }

    public TestCacheItem(string value)
    {
        Value = value;
    }
}

public class AnotherCacheItem
{
    public int Number { get; set; }
}

internal sealed class StubKeyNormalizer : IChengYuanCacheKeyNormalizer
{
    public string Normalize(ChengYuanCacheKey key) => $"test::{key.Value}";
}

internal sealed class NullCurrentTenant : ICurrentTenant
{
    public Guid? Id => null;
    public string? Name => null;
    public bool IsAvailable => false;
}

internal sealed class TestMemoryCacheStore(IMemoryCache memoryCache) : IChengYuanCacheStore
{
    public ValueTask<ChengYuanCacheItem?> GetAsync(string normalizedKey, CancellationToken cancellationToken = default)
    {
        memoryCache.TryGetValue(normalizedKey, out ChengYuanCacheItem? item);
        return ValueTask.FromResult(item);
    }

    public ValueTask SetAsync(
        string normalizedKey,
        ChengYuanCacheItem item,
        ChengYuanCacheEntryOptions options,
        CancellationToken cancellationToken = default)
    {
        var entryOptions = new MemoryCacheEntryOptions();
        if (options.AbsoluteExpirationRelativeToNow.HasValue)
            entryOptions.AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow;
        if (options.SlidingExpiration.HasValue)
            entryOptions.SlidingExpiration = options.SlidingExpiration;

        memoryCache.Set(normalizedKey, item, entryOptions);
        return ValueTask.CompletedTask;
    }

    public ValueTask RemoveAsync(string normalizedKey, CancellationToken cancellationToken = default)
    {
        memoryCache.Remove(normalizedKey);
        return ValueTask.CompletedTask;
    }
}
