using ChengYuan.ObjectMapping;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public sealed class ObjectMappingTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    public ObjectMappingTests()
    {
        var services = new ServiceCollection();
        services.AddObjectMapping();
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public void AddObjectMapping_RegistersIObjectMapper()
    {
        var mapper = _serviceProvider.GetRequiredService<IObjectMapper>();

        mapper.ShouldNotBeNull();
    }

    [Fact]
    public void DefaultMapper_WithNotConfiguredProvider_Throws()
    {
        var mapper = _serviceProvider.GetRequiredService<IObjectMapper>();

        Should.Throw<InvalidOperationException>(() => mapper.Map<SourceDto, DestDto>(new SourceDto { Value = "x" }));
    }

    [Fact]
    public void CustomMappingProvider_WorksCorrectly()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IObjectMappingProvider, TestMappingProvider>();
        services.AddObjectMapping();
        using var sp = services.BuildServiceProvider();
        var mapper = sp.GetRequiredService<IObjectMapper>();

        var result = mapper.Map<SourceDto, DestDto>(new SourceDto { Value = "hello" });

        result.ShouldNotBeNull();
        result.Value.ShouldBe("hello");
    }

    public void Dispose() => _serviceProvider.Dispose();
}

public sealed class SourceDto
{
    public required string Value { get; init; }
}

public sealed class DestDto
{
    public string? Value { get; set; }
}

public sealed class TestMappingProvider : IObjectMappingProvider
{
    public TDestination Map<TSource, TDestination>(TSource source)
    {
        if (source is SourceDto s && typeof(TDestination) == typeof(DestDto))
        {
            return (TDestination)(object)new DestDto { Value = s.Value };
        }

        throw new NotSupportedException();
    }

    public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
    {
        if (source is SourceDto s && destination is DestDto d)
        {
            d.Value = s.Value;
            return (TDestination)(object)d;
        }

        throw new NotSupportedException();
    }
}
