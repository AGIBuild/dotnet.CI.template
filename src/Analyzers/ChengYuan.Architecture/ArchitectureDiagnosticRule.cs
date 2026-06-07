namespace ChengYuan.Architecture;

internal sealed record ArchitectureDiagnosticRule(
    string Id,
    string Title,
    string Category,
    string Message,
    string Rationale);
