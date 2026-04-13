using ChengYuan.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public sealed class ObjectAccessorTests
{
    [Fact]
    public void AddObjectAccessor_ShouldStoreAndRetrieveValue()
    {
        var services = new ServiceCollection();
        var accessor = services.AddObjectAccessor("hello");

        accessor.Value.ShouldBe("hello");
        services.GetObject<string>().ShouldBe("hello");
    }

    [Fact]
    public void AddObjectAccessor_ShouldThrowOnDuplicate()
    {
        var services = new ServiceCollection();
        services.AddObjectAccessor("first");

        Should.Throw<InvalidOperationException>(() => services.AddObjectAccessor("second"));
    }

    [Fact]
    public void TryAddObjectAccessor_ShouldReturnExistingOnDuplicate()
    {
        var services = new ServiceCollection();
        var first = services.AddObjectAccessor("first");
        var second = services.TryAddObjectAccessor<string>();

        second.ShouldBeSameAs(first);
    }

    [Fact]
    public void GetObjectOrNull_ShouldReturnNullWhenNotRegistered()
    {
        var services = new ServiceCollection();

        services.GetObjectOrNull<string>().ShouldBeNull();
    }

    [Fact]
    public void GetObject_ShouldThrowWhenNotRegistered()
    {
        var services = new ServiceCollection();

        Should.Throw<InvalidOperationException>(() => services.GetObject<string>());
    }

    [Fact]
    public void ObjectAccessor_ShouldBeResolvableFromDI()
    {
        var services = new ServiceCollection();
        services.AddObjectAccessor("from-di");

        using var provider = services.BuildServiceProvider();
        var accessor = provider.GetRequiredService<IObjectAccessor<string>>();

        accessor.Value.ShouldBe("from-di");
    }

    [Fact]
    public void ObjectAccessor_MutatedValueShouldBeVisibleThroughDI()
    {
        var services = new ServiceCollection();
        var accessor = services.AddObjectAccessor<string>();
        accessor.Value = "mutated";

        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IObjectAccessor<string>>().Value.ShouldBe("mutated");
    }
}
