using ChengYuan.Sms;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public sealed class SmsTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    public SmsTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSms();
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public void AddSms_RegistersISmsSender()
    {
        var sender = _serviceProvider.GetRequiredService<ISmsSender>();

        sender.ShouldNotBeNull();
    }

    [Fact]
    public async Task NullSmsSender_DoesNotThrow()
    {
        var sender = _serviceProvider.GetRequiredService<ISmsSender>();

        var message = new SmsMessage("+61412345678", "Test message");

        await Should.NotThrowAsync(() => sender.SendAsync(message, TestContext.Current.CancellationToken));
    }

    public void Dispose() => _serviceProvider.Dispose();
}
