namespace ChengYuan.Core.Logging;

internal interface IInitLoggerEntries
{
    IReadOnlyList<InitLogEntry> Entries { get; }
}
