using ChengYuan.Core.Logging;
using Microsoft.Extensions.Logging;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class LoggingUtilityTests
{
    [Fact]
    public void HasLogLevelExtension_SetsLogLevel()
    {
        var exception = new TestExceptionWithLogLevel("test");

        var result = exception.WithLogLevel(LogLevel.Warning);

        result.LogLevel.ShouldBe(LogLevel.Warning);
        result.ShouldBeSameAs(exception);
    }

    [Fact]
    public void HasLogLevel_DefaultsToError()
    {
        var exception = new TestExceptionWithLogLevel("test");

        exception.LogLevel.ShouldBe(LogLevel.Error);
    }

    [Fact]
    public void ExceptionWithSelfLogging_CanLogCustomDetails()
    {
        var exception = new TestSelfLoggingException("test message", "extra-context");
        var testLogger = new BufferingLogger();

        exception.Log(testLogger);

        testLogger.Messages.Count.ShouldBe(1);
        testLogger.Messages[0].ShouldContain("extra-context");
    }

    private sealed class TestExceptionWithLogLevel(string message)
        : Exception(message), IHasLogLevel
    {
        public LogLevel LogLevel { get; set; } = LogLevel.Error;
    }

    private sealed class TestSelfLoggingException(string message, string context)
        : Exception(message), IExceptionWithSelfLogging
    {
        public void Log(ILogger logger)
        {
            logger.Log(LogLevel.Warning, default, $"Self-logged error with context: {context}", null, static (s, _) => s);
        }
    }

    private sealed class BufferingLogger : ILogger
    {
        public List<string> Messages { get; } = [];

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    }
}
