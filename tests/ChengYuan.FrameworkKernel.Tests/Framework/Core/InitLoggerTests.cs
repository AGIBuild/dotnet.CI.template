using ChengYuan.Core.Logging;
using Microsoft.Extensions.Logging;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class InitLoggerTests
{
    [Fact]
    public void Create_ReturnsSameInstanceForSameType()
    {
        var factory = new DefaultInitLoggerFactory();

        var logger1 = factory.Create<InitLoggerTests>();
        var logger2 = factory.Create<InitLoggerTests>();

        logger1.ShouldBeSameAs(logger2);
    }

    [Fact]
    public void Create_ReturnsDifferentInstancesForDifferentTypes()
    {
        var factory = new DefaultInitLoggerFactory();

        var logger1 = factory.Create<InitLoggerTests>();
        var logger2 = factory.Create<DefaultInitLoggerFactory>();

        logger1.ShouldNotBeSameAs(logger2);
    }

    [Fact]
    public void Log_BuffersEntries()
    {
        var factory = new DefaultInitLoggerFactory();
        var logger = factory.Create<InitLoggerTests>();

        logger.Log(LogLevel.Information, default, "Module TestModule loaded", null, static (s, _) => s);
        logger.Log(LogLevel.Warning, default, "Something might be wrong", null, static (s, _) => s);

        logger.Entries.Count.ShouldBe(2);
        logger.Entries[0].LogLevel.ShouldBe(LogLevel.Information);
        logger.Entries[0].Message.ShouldContain("TestModule");
        logger.Entries[1].LogLevel.ShouldBe(LogLevel.Warning);
    }

    [Fact]
    public void Log_CapturesException()
    {
        var factory = new DefaultInitLoggerFactory();
        var logger = factory.Create<InitLoggerTests>();
        var ex = new InvalidOperationException("test error");

        logger.Log(LogLevel.Error, default, "Failed to load", ex, static (s, _) => s);

        logger.Entries.Count.ShouldBe(1);
        logger.Entries[0].Exception.ShouldBeSameAs(ex);
        logger.Entries[0].LogLevel.ShouldBe(LogLevel.Error);
    }

    [Fact]
    public void IsEnabled_ReturnsTrueForAllLevelsExceptNone()
    {
        var factory = new DefaultInitLoggerFactory();
        var logger = factory.Create<InitLoggerTests>();

        logger.IsEnabled(LogLevel.Trace).ShouldBeTrue();
        logger.IsEnabled(LogLevel.Debug).ShouldBeTrue();
        logger.IsEnabled(LogLevel.Information).ShouldBeTrue();
        logger.IsEnabled(LogLevel.Warning).ShouldBeTrue();
        logger.IsEnabled(LogLevel.Error).ShouldBeTrue();
        logger.IsEnabled(LogLevel.Critical).ShouldBeTrue();
        logger.IsEnabled(LogLevel.None).ShouldBeFalse();
    }

    [Fact]
    public void GetAllEntries_AggregatesFromAllLoggers()
    {
        var factory = new DefaultInitLoggerFactory();
        var logger1 = factory.Create<InitLoggerTests>();
        var logger2 = factory.Create<DefaultInitLoggerFactory>();

        logger1.Log(LogLevel.Information, default, "Entry 1", null, static (s, _) => s);
        logger2.Log(LogLevel.Warning, default, "Entry 2", null, static (s, _) => s);
        logger1.Log(LogLevel.Error, default, "Entry 3", null, static (s, _) => s);

        var allEntries = factory.GetAllEntries();
        allEntries.Count.ShouldBe(3);
    }

    [Fact]
    public void ClearAllEntries_RemovesAllLoggers()
    {
        var factory = new DefaultInitLoggerFactory();
        var logger = factory.Create<InitLoggerTests>();
        logger.Log(LogLevel.Information, default, "Some entry", null, static (s, _) => s);

        factory.ClearAllEntries();

        factory.GetAllEntries().Count.ShouldBe(0);
    }

    [Fact]
    public void Entries_ContainCorrectCategoryName()
    {
        var factory = new DefaultInitLoggerFactory();
        var logger = factory.Create<InitLoggerTests>();

        logger.Log(LogLevel.Information, default, "test", null, static (s, _) => s);

        logger.Entries[0].CategoryName.ShouldBe(typeof(InitLoggerTests).FullName);
    }

    [Fact]
    public void BeginScope_ReturnsNonNullDisposable()
    {
        var factory = new DefaultInitLoggerFactory();
        var logger = factory.Create<InitLoggerTests>();

        using var scope = logger.BeginScope("test scope");
        scope.ShouldNotBeNull();
    }
}
