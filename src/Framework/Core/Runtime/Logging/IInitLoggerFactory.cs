namespace ChengYuan.Core.Logging;

public interface IInitLoggerFactory
{
    IInitLogger<T> Create<T>();

    IReadOnlyList<InitLogEntry> GetAllEntries();

    void ClearAllEntries();
}
