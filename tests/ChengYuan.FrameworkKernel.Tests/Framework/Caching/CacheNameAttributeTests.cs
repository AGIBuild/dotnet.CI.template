using ChengYuan.Caching;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class CacheNameAttributeTests
{
    [Fact]
    public void GetCacheName_ReturnsAttributeName_WhenPresent()
    {
        var name = CacheNameAttribute.GetCacheName<CustomNamedCacheItem>();

        name.ShouldBe("my-custom-cache");
    }

    [Fact]
    public void GetCacheName_StripsPostfix_WhenNoAttribute()
    {
        var name = CacheNameAttribute.GetCacheName<TestCacheItem>();

        name.ShouldEndWith("Test");
        name.ShouldNotContain("CacheItem");
    }

    [Fact]
    public void GetCacheName_ReturnsFullName_WhenNeitherAttributeNorPostfix()
    {
        var name = CacheNameAttribute.GetCacheName<PlainDto>();

        name.ShouldBe(typeof(PlainDto).FullName);
    }

    [Fact]
    public void GetCacheName_Generic_MatchesNonGeneric()
    {
        var generic = CacheNameAttribute.GetCacheName<CustomNamedCacheItem>();
#pragma warning disable CA2263 // Testing the non-generic overload specifically
        var nonGeneric = CacheNameAttribute.GetCacheName(typeof(CustomNamedCacheItem));
#pragma warning restore CA2263

        generic.ShouldBe(nonGeneric);
    }

    [CacheName("my-custom-cache")]
    private sealed class CustomNamedCacheItem;

    private sealed class PlainDto;
}
