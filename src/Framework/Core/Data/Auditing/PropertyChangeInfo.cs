namespace ChengYuan.Core.Data.Auditing;

public sealed record PropertyChangeInfo(string PropertyName, object? OriginalValue, object? NewValue);
