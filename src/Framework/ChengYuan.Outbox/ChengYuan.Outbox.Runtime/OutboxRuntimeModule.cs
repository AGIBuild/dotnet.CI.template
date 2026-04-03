using System.Text.Json;
using ChengYuan.Core.Modularity;
using ChengYuan.Core.Timing;
using ChengYuan.ExecutionContext;
using ChengYuan.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Outbox;

public static class OutboxServiceCollectionExtensions
{
    public static IServiceCollection AddOutboxRuntime(this IServiceCollection services)
    {
        services.AddSingleton<IOutboxSerializer, SystemTextJsonOutboxSerializer>();
        services.AddSingleton<IOutbox, DefaultOutbox>();

        return services;
    }
}

[DependsOn(typeof(ExecutionContextModule))]
[DependsOn(typeof(MultiTenancyModule))]
public sealed class OutboxModule : ModuleBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddOutboxRuntime();
    }
}

internal sealed class DefaultOutbox(
    IOutboxStore store,
    IOutboxSerializer serializer,
    IClock clock,
    ICurrentTenant currentTenant,
    ICurrentCorrelation currentCorrelation) : IOutbox
{
    public async ValueTask<Guid> EnqueueAsync<T>(string name, T payload, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(payload);

        var messageId = Guid.NewGuid();
        var message = new OutboxMessage(
            messageId,
            name,
            serializer.Serialize(payload),
            clock.UtcNow,
            currentTenant.Id,
            currentCorrelation.CorrelationId,
            OutboxMessageStatus.Pending,
            0,
            null,
            null);

        await store.SaveAsync(message, cancellationToken);
        return messageId;
    }
}

internal sealed class SystemTextJsonOutboxSerializer : IOutboxSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public OutboxPayload Serialize<T>(T payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        var type = typeof(T);
        return new OutboxPayload(
            JsonSerializer.SerializeToUtf8Bytes(payload, SerializerOptions),
            type.AssemblyQualifiedName ?? type.FullName ?? type.Name);
    }

    public T? Deserialize<T>(OutboxPayload payload)
    {
        return JsonSerializer.Deserialize<T>(payload.Content, SerializerOptions);
    }
}
