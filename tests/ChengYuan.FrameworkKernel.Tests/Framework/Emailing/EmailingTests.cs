using ChengYuan.Emailing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public sealed class EmailingTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    public EmailingTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddEmailing();
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public void AddEmailing_RegistersIEmailSender()
    {
        var sender = _serviceProvider.GetRequiredService<IEmailSender>();

        sender.ShouldNotBeNull();
    }

    [Fact]
    public async Task NullEmailSender_DoesNotThrow()
    {
        var sender = _serviceProvider.GetRequiredService<IEmailSender>();

        var message = new EmailMessage
        {
            To = "test@example.com",
            Subject = "Test",
            Body = "Body"
        };

        await Should.NotThrowAsync(() => sender.SendAsync(message, TestContext.Current.CancellationToken));
    }

    public void Dispose() => _serviceProvider.Dispose();
}
