using ChengYuan.ObjectMapping;
using ChengYuan.ObjectMapping.Mapster;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests.Framework.ObjectMapping;

public sealed class MapsterObjectMappingTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    public MapsterObjectMappingTests()
    {
        var services = new ServiceCollection();
        services.AddMapsterObjectMapping();
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public void AddMapsterObjectMapping_Registers_IObjectMapper()
    {
        var mapper = _serviceProvider.GetRequiredService<IObjectMapper>();
        mapper.ShouldNotBeNull();
    }

    [Fact]
    public void Map_MatchingProperties_Succeeds()
    {
        var mapper = _serviceProvider.GetRequiredService<IObjectMapper>();
        var source = new MapsterSource { Name = "test", Age = 42 };

        var destination = mapper.Map<MapsterSource, MapsterTarget>(source);

        destination.ShouldNotBeNull();
        destination.Name.ShouldBe("test");
        destination.Age.ShouldBe(42);
    }

    [Fact]
    public void Map_ToExistingObject_UpdatesProperties()
    {
        var mapper = _serviceProvider.GetRequiredService<IObjectMapper>();
        var source = new MapsterSource { Name = "updated", Age = 99 };
        var target = new MapsterTarget { Name = "original", Age = 1 };

        var result = mapper.Map(source, target);

        result.ShouldBeSameAs(target);
        result.Name.ShouldBe("updated");
        result.Age.ShouldBe(99);
    }

    public void Dispose() => _serviceProvider.Dispose();
}

public sealed class MapsterSource
{
    public string? Name { get; set; }

    public int Age { get; set; }
}

public sealed class MapsterTarget
{
    public string? Name { get; set; }

    public int Age { get; set; }
}
