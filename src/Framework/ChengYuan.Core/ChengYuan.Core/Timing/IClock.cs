namespace ChengYuan.Core.Timing;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
