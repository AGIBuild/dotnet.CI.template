using ChengYuan.Core;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class CoreHelperTests
{
    [Fact]
    public void NotEmpty_ShouldRejectEmptyGuids()
    {
        Should.Throw<ArgumentException>(() => Check.NotEmpty(Guid.Empty, "workspaceId"));
    }

    [Fact]
    public void NotDefault_ShouldRejectDefaultStructValues()
    {
        Should.Throw<ArgumentException>(() => Check.NotDefault(default(int), "version"));
    }

    [Fact]
    public void NotNullOrEmpty_ShouldRejectEmptyCollections()
    {
        Should.Throw<ArgumentException>(() => Check.NotNullOrEmpty(Array.Empty<string>(), "items"));
    }

    [Fact]
    public void Positive_ShouldRejectZeroAndNegativeValues()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => Check.Positive(0, "maxCount"));
        Should.Throw<ArgumentOutOfRangeException>(() => Check.Positive(-1, "maxCount"));
    }

    [Fact]
    public void NotNegative_ShouldAllowZeroAndPositiveValues()
    {
        Check.NotNegative(0, "retryCount").ShouldBe(0);
        Check.NotNegative(2, "retryCount").ShouldBe(2);
        Should.Throw<ArgumentOutOfRangeException>(() => Check.NotNegative(-1, "retryCount"));
    }

    [Fact]
    public void CheckHelpers_ShouldReturnTheOriginalValue_WhenValid()
    {
        var workspaceId = Guid.NewGuid();
        var items = new[] { "core" };

        Check.NotEmpty(workspaceId, "workspaceId").ShouldBe(workspaceId);
        Check.NotDefault(3, "version").ShouldBe(3);
        Check.NotNullOrEmpty(items, "items").ShouldBe(items);
        Check.Positive(5, "maxCount").ShouldBe(5);
    }
}
