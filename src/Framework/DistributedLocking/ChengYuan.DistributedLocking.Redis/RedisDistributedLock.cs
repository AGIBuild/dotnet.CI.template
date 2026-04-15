using StackExchange.Redis;

namespace ChengYuan.DistributedLocking;

public sealed class RedisDistributedLock(IConnectionMultiplexer connectionMultiplexer) : IDistributedLock
{
    private const string KeyPrefix = "distributed-lock:";

    private static readonly LuaScript ReleaseScript = LuaScript.Prepare(
        "if redis.call('get', @key) == @value then return redis.call('del', @key) else return 0 end");

    public async Task<IAsyncDisposable?> TryAcquireAsync(
        string name,
        TimeSpan timeout = default,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var db = connectionMultiplexer.GetDatabase();
        var key = KeyPrefix + name;
        var value = Guid.NewGuid().ToString("N");
        var expiry = timeout > TimeSpan.Zero ? timeout : TimeSpan.FromSeconds(30);

        var acquired = await db.StringSetAsync(key, value, expiry, When.NotExists);
        if (!acquired)
        {
            return null;
        }

        return new RedisLockHandle(db, key, value);
    }

    private sealed class RedisLockHandle(IDatabase db, string key, string value) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            await db.ScriptEvaluateAsync(ReleaseScript, new { key = (RedisKey)key, value = (RedisValue)value });
        }
    }
}
