using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.EventBus;

public sealed class LocalEventBus(IServiceProvider serviceProvider) : ILocalEventBus
{
    private static readonly ConcurrentDictionary<Type, MethodInfo> HandleMethodCache = new();

    public ValueTask PublishAsync<TEvent>(TEvent eventData, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        ArgumentNullException.ThrowIfNull(eventData);

        return PublishAsync(typeof(TEvent), eventData, cancellationToken);
    }

    public async ValueTask PublishAsync(Type eventType, object eventData, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventType);
        ArgumentNullException.ThrowIfNull(eventData);

        var subscriberType = typeof(IEventSubscriber<>).MakeGenericType(eventType);
        var subscribers = serviceProvider.GetServices(subscriberType);

        var method = HandleMethodCache.GetOrAdd(eventType, _ =>
            subscriberType.GetMethod(nameof(IEventSubscriber<object>.HandleAsync))
            ?? throw new InvalidOperationException($"Method '{nameof(IEventSubscriber<object>.HandleAsync)}' not found on subscriber type '{subscriberType.FullName}'."));

        List<Exception>? exceptions = null;

        foreach (var subscriber in subscribers)
        {
            if (subscriber is null)
            {
                continue;
            }

            try
            {
                var result = method.Invoke(subscriber, [eventData, cancellationToken]);

                if (result is ValueTask valueTask)
                {
                    await valueTask;
                }
            }
            catch (TargetInvocationException ex) when (ex.InnerException is not null)
            {
                exceptions ??= [];
                exceptions.Add(ex.InnerException);
            }
            catch (Exception ex)
            {
                exceptions ??= [];
                exceptions.Add(ex);
            }
        }

        if (exceptions is { Count: > 0 })
        {
            throw new AggregateException(
                $"One or more errors occurred while handling event '{eventType.FullName}'.",
                exceptions);
        }
    }
}
