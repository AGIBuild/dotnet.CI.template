using ChengYuan.Core.GlobalFeatures;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public sealed class GlobalFeatureManagerTests : IDisposable
{
    [Fact]
    public void IsEnabled_ReturnsFalse_ByDefault()
    {
        GlobalFeatureManager.IsEnabled<TestGlobalFeature>().ShouldBeFalse();
    }

    [Fact]
    public void Enable_ThenIsEnabled_ReturnsTrue()
    {
        GlobalFeatureManager.Enable<TestGlobalFeature>();

        GlobalFeatureManager.IsEnabled<TestGlobalFeature>().ShouldBeTrue();
    }

    [Fact]
    public void Disable_ThenIsEnabled_ReturnsFalse()
    {
        GlobalFeatureManager.Enable<TestGlobalFeature>();
        GlobalFeatureManager.Disable<TestGlobalFeature>();

        GlobalFeatureManager.IsEnabled<TestGlobalFeature>().ShouldBeFalse();
    }

    [Fact]
    public void GetOrAdd_ReturnsSameInstance()
    {
        var feature1 = GlobalFeatureManager.GetOrAdd<TestGlobalFeature>();
        var feature2 = GlobalFeatureManager.GetOrAdd<TestGlobalFeature>();

        feature1.ShouldBeSameAs(feature2);
    }

    [Fact]
    public void GetAll_ReturnsRegisteredFeatures()
    {
        GlobalFeatureManager.Enable<TestGlobalFeature>();

        var all = GlobalFeatureManager.GetAll();

        all.ShouldContain(f => f is TestGlobalFeature);
    }

    public void Dispose()
    {
        // Reset: disable to restore state
        GlobalFeatureManager.Disable<TestGlobalFeature>();
    }
}

public sealed class TestGlobalFeature : GlobalFeature
{
    public override string FeatureName => "TestFeature";
}
