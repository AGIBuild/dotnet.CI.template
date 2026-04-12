using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace ChengYuan.Caching;

internal sealed partial class DefaultChengYuanTypedCache<TCacheItem> : IChengYuanCache<TCacheItem>, IDisposable
    where TCacheItem : class
{
    private readonly IChengYuanCacheStore _store;
    private readonly IChengYuanCacheKeyNormalizer _keyNormalizer;
    private readonly IChengYuanCacheSerializer _serializer;
    private readonly ChengYuanCacheOptions _options;
    private readonly ILogger _logger;
    private readonly string _cacheName;
    private readonly ChengYuanCacheEntryOptions _defaultEntryOptions;
    private readonly SemaphoreSlim _syncSemaphore = new(1, 1);

    public DefaultChengYuanTypedCache(
        IChengYuanCacheStore store,
        IChengYuanCacheKeyNormalizer keyNormalizer,
        IChengYuanCacheSerializer serializer,
        IOptions<ChengYuanCacheOptions> options,
        ILoggerFactory? loggerFactory = null)
    {
        _store = store;
        _keyNormalizer = keyNormalizer;
        _serializer = serializer;
        _options = options.Value;
        _logger = loggerFactory?.CreateLogger(GetType()) ?? NullLogger.Instance;
        _cacheName = CacheNameAttribute.GetCacheName<TCacheItem>();
        _defaultEntryOptions = ResolveDefaultEntryOptions();
    }

    public async ValueTask<TCacheItem?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        var normalizedKey = NormalizeKey(key);

        try
        {
            var item = await _store.GetAsync(normalizedKey, cancellationToken);
            return item is null ? default : _serializer.Deserialize<TCacheItem>(item);
        }
        catch (Exception ex) when (ShouldHideError(ex))
        {
            LogCacheError(ex, nameof(GetAsync));
            return default;
        }
    }

    public async ValueTask SetAsync(
        string key,
        TCacheItem value,
        ChengYuanCacheEntryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(value);

        var normalizedKey = NormalizeKey(key);
        var item = _serializer.Serialize(value);

        try
        {
            await _store.SetAsync(normalizedKey, item, options ?? _defaultEntryOptions, cancellationToken);
        }
        catch (Exception ex) when (ShouldHideError(ex))
        {
            LogCacheError(ex, nameof(SetAsync));
        }
    }

    public async ValueTask<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        var normalizedKey = NormalizeKey(key);

        try
        {
            return await _store.GetAsync(normalizedKey, cancellationToken) is not null;
        }
        catch (Exception ex) when (ShouldHideError(ex))
        {
            LogCacheError(ex, nameof(ExistsAsync));
            return false;
        }
    }

    public async ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        var normalizedKey = NormalizeKey(key);

        try
        {
            await _store.RemoveAsync(normalizedKey, cancellationToken);
        }
        catch (Exception ex) when (ShouldHideError(ex))
        {
            LogCacheError(ex, nameof(RemoveAsync));
        }
    }

    public async ValueTask<TCacheItem> GetOrCreateAsync(
        string key,
        Func<CancellationToken, ValueTask<TCacheItem>> factory,
        ChengYuanCacheEntryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(factory);

        var value = await GetAsync(key, cancellationToken);
        if (value is not null)
        {
            return value;
        }

        await _syncSemaphore.WaitAsync(cancellationToken);
        try
        {
            value = await GetAsync(key, cancellationToken);
            if (value is not null)
            {
                return value;
            }

            value = await factory(cancellationToken);
            await SetAsync(key, value, options, cancellationToken);
            return value;
        }
        finally
        {
            _syncSemaphore.Release();
        }
    }

    private string NormalizeKey(string key)
    {
        var cacheKey = new ChengYuanCacheKey(
            string.IsNullOrEmpty(_options.KeyPrefix)
                ? $"{_cacheName}::{key}"
                : $"{_options.KeyPrefix}{_cacheName}::{key}");

        return _keyNormalizer.Normalize(cacheKey);
    }

    private ChengYuanCacheEntryOptions ResolveDefaultEntryOptions()
    {
        foreach (var configurator in _options.CacheConfigurators)
        {
            var result = configurator(_cacheName);
            if (result is not null)
            {
                return result;
            }
        }

        return _options.GlobalCacheEntryOptions;
    }

    private bool ShouldHideError(Exception ex)
    {
        return _options.HideErrors && ex is not OperationCanceledException;
    }

    private void LogCacheError(Exception ex, string operation)
    {
        LogCacheWarning(_logger, _cacheName, operation, ex);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Cache '{CacheName}' encountered an error during '{Operation}'.")]
    private static partial void LogCacheWarning(ILogger logger, string cacheName, string operation, Exception ex);

    public void Dispose()
    {
        _syncSemaphore.Dispose();
    }
}
