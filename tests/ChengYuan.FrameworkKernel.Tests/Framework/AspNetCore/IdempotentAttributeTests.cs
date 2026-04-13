using ChengYuan.AspNetCore;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public sealed class IdempotentAttributeTests
{
    [Fact]
    public void Defaults_AreCorrect()
    {
        var attr = new IdempotentAttribute();

        attr.HeaderName.ShouldBe("Idempotency-Key");
        attr.CacheSeconds.ShouldBe(86400);
    }

    [Fact]
    public void Properties_CanBeCustomized()
    {
        var attr = new IdempotentAttribute
        {
            HeaderName = "X-Request-Id",
            CacheSeconds = 3600,
        };

        attr.HeaderName.ShouldBe("X-Request-Id");
        attr.CacheSeconds.ShouldBe(3600);
    }
}
