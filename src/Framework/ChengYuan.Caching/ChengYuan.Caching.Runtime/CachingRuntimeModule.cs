using System.Text.Json;
using ChengYuan.Core.Modularity;
using ChengYuan.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Caching;

public static class CachingServiceCollectionExtensions
{
    public static IServiceCollection AddCaching(this IServiceCollection services)
    {
        services.AddSingleton<IChengYuanCacheKeyNormalizer, DefaultChengYuanCacheKeyNormalizer>();
        services.AddSingleton<IChengYuanCacheSerializer, SystemTextJsonChengYuanCacheSerializer>();
        services.AddSingleton<IChengYuanCache, DefaultChengYuanCache>();

        return services;
    }
}

[DependsOn(typeof(MultiTenancyModule))]
public sealed class CachingModule : ModuleBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddCaching();
    }
}

internal sealed class DefaultChengYuanCache(
    IChengYuanCacheStore store,
    IChengYuanCacheKeyNormalizer keyNormalizer,
    IChengYuanCacheSerializer serializer) : IChengYuanCache
{
    public async ValueTask<T?> GetAsync<T>(ChengYuanCacheKey key, CancellationToken cancellationToken = default)
    {
        var normalizedKey = keyNormalizer.Normalize(key);
        var item = await store.GetAsync(normalizedKey, cancellationToken);

        return item is null
            ? default
            : serializer.Deserialize<T>(item);
    }

    public async ValueTask SetAsync<T>(
        ChengYuanCacheKey key,
        T value,
        ChengYuanCacheEntryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(value);

        var normalizedKey = keyNormalizer.Normalize(key);
        var item = serializer.Serialize(value);

        await store.SetAsync(normalizedKey, item, options ?? new ChengYuanCacheEntryOptions(), cancellationToken);
    }

    public async ValueTask<bool> ExistsAsync(ChengYuanCacheKey key, CancellationToken cancellationToken = default)
    {
        var normalizedKey = keyNormalizer.Normalize(key);
        return await store.GetAsync(normalizedKey, cancellationToken) is not null;
    }

    public async ValueTask RemoveAsync(ChengYuanCacheKey key, CancellationToken cancellationToken = default)
    {
        var normalizedKey = keyNormalizer.Normalize(key);
        await store.RemoveAsync(normalizedKey, cancellationToken);
    }

    public async ValueTask<T> GetOrCreateAsync<T>(
        ChengYuanCacheKey key,
        Func<CancellationToken, ValueTask<T>> factory,
        ChengYuanCacheEntryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(factory);

        var normalizedKey = keyNormalizer.Normalize(key);
        var existingItem = await store.GetAsync(normalizedKey, cancellationToken);
        if (existingItem is not null)
            return serializer.Deserialize<T>(existingItem)!;

        var created = await factory(cancellationToken);
        await SetAsync(key, created, options, cancellationToken);

        return created;
    }
}

internal sealed class DefaultChengYuanCacheKeyNormalizer(ICurrentTenant currentTenant) : IChengYuanCacheKeyNormalizer
{
    public string Normalize(ChengYuanCacheKey key)
    {
        return key.Scope switch
        {
            ChengYuanCacheScope.Global => $"global::{key.Value}",
            ChengYuanCacheScope.Tenant => currentTenant.Id is Guid tenantId
                ? $"tenant::{tenantId:N}::{key.Value}"
                : throw new InvalidOperationException("Tenant-scoped cache access requires an active tenant context."),
            _ => throw new InvalidOperationException($"Unsupported cache scope '{key.Scope}'.")
        };
    }
}

internal sealed class SystemTextJsonChengYuanCacheSerializer : IChengYuanCacheSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public ChengYuanCacheItem Serialize<T>(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var type = typeof(T);
        return new ChengYuanCacheItem(
            JsonSerializer.SerializeToUtf8Bytes(value, SerializerOptions),
            type.AssemblyQualifiedName ?? type.FullName ?? type.Name);
    }

    public T? Deserialize<T>(ChengYuanCacheItem item)
    {
        return JsonSerializer.Deserialize<T>(item.Payload, SerializerOptions);
    }
}
