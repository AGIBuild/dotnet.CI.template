using System.Linq.Expressions;
using ChengYuan.Core.Specifications;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public sealed class SpecificationTests
{
    [Fact]
    public void IsSatisfiedBy_ReturnsTrueForMatchingEntity()
    {
        var spec = new GreaterThanSpec(10);

        spec.IsSatisfiedBy(15).ShouldBeTrue();
        spec.IsSatisfiedBy(5).ShouldBeFalse();
    }

    [Fact]
    public void And_CombinesSpecificationsCorrectly()
    {
        var greaterThan5 = new GreaterThanSpec(5);
        var lessThan20 = new LessThanSpec(20);
        var combined = greaterThan5 & lessThan20;

        combined.IsSatisfiedBy(10).ShouldBeTrue();
        combined.IsSatisfiedBy(3).ShouldBeFalse();
        combined.IsSatisfiedBy(25).ShouldBeFalse();
    }

    [Fact]
    public void Or_CombinesSpecificationsCorrectly()
    {
        var lessThan5 = new LessThanSpec(5);
        var greaterThan20 = new GreaterThanSpec(20);
        var combined = lessThan5 | greaterThan20;

        combined.IsSatisfiedBy(3).ShouldBeTrue();
        combined.IsSatisfiedBy(25).ShouldBeTrue();
        combined.IsSatisfiedBy(10).ShouldBeFalse();
    }

    [Fact]
    public void Not_InvertsSpecification()
    {
        var greaterThan10 = new GreaterThanSpec(10);
        var notGreaterThan10 = !greaterThan10;

        notGreaterThan10.IsSatisfiedBy(5).ShouldBeTrue();
        notGreaterThan10.IsSatisfiedBy(15).ShouldBeFalse();
    }

    [Fact]
    public void Complex_CombinedSpecification()
    {
        // (x > 5 AND x < 20) OR x == 100
        var inRange = new GreaterThanSpec(5) & new LessThanSpec(20);
        var isSpecial = new EqualSpec(100);
        var combined = inRange | isSpecial;

        combined.IsSatisfiedBy(10).ShouldBeTrue();
        combined.IsSatisfiedBy(100).ShouldBeTrue();
        combined.IsSatisfiedBy(3).ShouldBeFalse();
        combined.IsSatisfiedBy(50).ShouldBeFalse();
    }

    private sealed class GreaterThanSpec(int threshold) : Specification<int>
    {
        public override Expression<Func<int, bool>> ToExpression() => x => x > threshold;
    }

    private sealed class LessThanSpec(int threshold) : Specification<int>
    {
        public override Expression<Func<int, bool>> ToExpression() => x => x < threshold;
    }

    private sealed class EqualSpec(int value) : Specification<int>
    {
        public override Expression<Func<int, bool>> ToExpression() => x => x == value;
    }
}
