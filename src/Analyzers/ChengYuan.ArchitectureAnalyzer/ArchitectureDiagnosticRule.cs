namespace ChengYuan.ArchitectureAnalyzer;

internal sealed class ArchitectureDiagnosticRule
{
    public ArchitectureDiagnosticRule(string id, string title, string category, string message, string rationale)
    {
        Id = id;
        Title = title;
        Category = category;
        Message = message;
        Rationale = rationale;
    }

    public string Id { get; }

    public string Title { get; }

    public string Category { get; }

    public string Message { get; }

    public string Rationale { get; }
}
