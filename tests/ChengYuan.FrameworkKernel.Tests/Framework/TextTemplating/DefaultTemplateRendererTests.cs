using ChengYuan.TextTemplating;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public sealed class DefaultTemplateRendererTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly ITemplateRenderer _renderer;

    public DefaultTemplateRendererTests()
    {
        var services = new ServiceCollection();
        services.AddTextTemplating();
        services.Configure<TextTemplatingOptions>(opts =>
        {
            opts.DefineTemplate("Welcome", defaultContent: "Hello, {{Name}}! Welcome to {{Place}}.");
            opts.DefineTemplate("NoModel", defaultContent: "Static content.");
        });
        _serviceProvider = services.BuildServiceProvider();
        _renderer = _serviceProvider.GetRequiredService<ITemplateRenderer>();
    }

    [Fact]
    public async Task RenderAsync_ReplacesPlaceholders()
    {
        var result = await _renderer.RenderAsync("Welcome", new { Name = "Alice", Place = "ChengYuan" }, TestContext.Current.CancellationToken);

        result.ShouldBe("Hello, Alice! Welcome to ChengYuan.");
    }

    [Fact]
    public async Task RenderAsync_WithNullModel_ReturnsRawContent()
    {
        var result = await _renderer.RenderAsync("NoModel", cancellationToken: TestContext.Current.CancellationToken);

        result.ShouldBe("Static content.");
    }

    [Fact]
    public async Task RenderAsync_ThrowsForUnknownTemplate()
    {
        await Should.ThrowAsync<InvalidOperationException>(
            () => _renderer.RenderAsync("NonExistent", cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task RenderStringAsync_ReplacesPlaceholders()
    {
        var result = await _renderer.RenderStringAsync("Hi {{Name}}", new { Name = "Bob" }, TestContext.Current.CancellationToken);

        result.ShouldBe("Hi Bob");
    }

    public void Dispose() => _serviceProvider.Dispose();
}
