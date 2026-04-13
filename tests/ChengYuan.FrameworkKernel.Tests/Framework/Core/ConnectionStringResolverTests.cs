using ChengYuan.Core.Data;
using Microsoft.Extensions.Options;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public sealed class ConnectionStringResolverTests
{
    [Fact]
    public async Task ResolveAsync_ReturnsDefault_WhenNameIsNull()
    {
        var resolver = CreateResolver("Server=default");

        var result = await resolver.ResolveAsync(cancellationToken: TestContext.Current.CancellationToken);

        result.ShouldBe("Server=default");
    }

    [Fact]
    public async Task ResolveAsync_ReturnsNamedConnection_WhenExists()
    {
        var resolver = CreateResolver("Server=default", ("Tenant1", "Server=tenant1"));

        var result = await resolver.ResolveAsync("Tenant1", TestContext.Current.CancellationToken);

        result.ShouldBe("Server=tenant1");
    }

    [Fact]
    public async Task ResolveAsync_FallsBackToDefault_WhenNameNotFound()
    {
        var resolver = CreateResolver("Server=fallback");

        var result = await resolver.ResolveAsync("NonExistent", TestContext.Current.CancellationToken);

        result.ShouldBe("Server=fallback");
    }

    [Fact]
    public async Task ResolveAsync_ReturnsNull_WhenNoDefaultAndNameNotFound()
    {
        var resolver = CreateResolver(null);

        var result = await resolver.ResolveAsync("Missing", TestContext.Current.CancellationToken);

        result.ShouldBeNull();
    }

    private static DefaultConnectionStringResolver CreateResolver(string? defaultConn, params (string Name, string Value)[] named)
    {
        var options = new ConnectionStringOptions { Default = defaultConn };
        foreach (var (name, value) in named)
        {
            options.ConnectionStrings[name] = value;
        }

        return new DefaultConnectionStringResolver(Options.Create(options));
    }
}
