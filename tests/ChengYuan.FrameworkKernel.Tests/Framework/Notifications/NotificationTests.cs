using ChengYuan.Notifications;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public sealed class NotificationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    public NotificationTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddNotifications();
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public void AddNotifications_RegistersINotificationSender()
    {
        var sender = _serviceProvider.GetRequiredService<INotificationSender>();

        sender.ShouldNotBeNull();
    }

    [Fact]
    public async Task NullChannel_DoesNotThrow()
    {
        var sender = _serviceProvider.GetRequiredService<INotificationSender>();

        var notification = new Notification
        {
            Name = "TestNotification",
            UserIds = ["user1", "user2"],
            Severity = NotificationSeverity.Info,
        };

        await Should.NotThrowAsync(() => sender.SendAsync(notification, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CustomChannel_ReceivesNotification()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddNotifications();
        var trackingChannel = new TrackingNotificationChannel();
        services.AddSingleton<INotificationChannel>(trackingChannel);
        using var sp = services.BuildServiceProvider();
        var sender = sp.GetRequiredService<INotificationSender>();

        var notification = new Notification
        {
            Name = "OrderCreated",
            UserIds = ["user-001"],
        };

        await sender.SendAsync(notification, TestContext.Current.CancellationToken);

        trackingChannel.SentNotifications.ShouldContain(n => n.Name == "OrderCreated");
    }

    public void Dispose() => _serviceProvider.Dispose();

    private sealed class TrackingNotificationChannel : INotificationChannel
    {
        public string Name => "Tracking";
        public List<Notification> SentNotifications { get; } = [];

        public Task SendAsync(Notification notification, IReadOnlyList<string> userIds, CancellationToken cancellationToken = default)
        {
            SentNotifications.Add(notification);
            return Task.CompletedTask;
        }
    }
}
