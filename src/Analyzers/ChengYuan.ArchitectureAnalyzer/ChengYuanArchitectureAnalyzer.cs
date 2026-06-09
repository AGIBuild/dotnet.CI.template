using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ChengYuan.ArchitectureAnalyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ChengYuanArchitectureAnalyzer : DiagnosticAnalyzer
{
    private const string HelpLinkBaseUri = "https://github.com/chengyuan/dotnet-template/tree/main/src/Analyzers/ChengYuan.ArchitectureAnalyzer";

    private static readonly DiagnosticDescriptor WebFacetMustNotReferencePersistence = CreateDescriptor(
        ArchitectureDiagnosticIds.WebFacetMustNotReferencePersistence);

    private static readonly DiagnosticDescriptor ProviderFacetMustNotReferenceHosting = CreateDescriptor(
        ArchitectureDiagnosticIds.ProviderFacetMustNotReferenceHosting);

    private static readonly DiagnosticDescriptor ModuleDependencyMustRespectCategoryOrder = CreateDescriptor(
        ArchitectureDiagnosticIds.ModuleDependencyMustRespectCategoryOrder);

    private static readonly ImmutableArray<string> ProviderAssemblyNames =
        ImmutableArray.Create(
            "ChengYuan.Data.Sqlite",
            "ChengYuan.Data.PostgreSql",
            "ChengYuan.Caching.Memory",
            "ChengYuan.Caching.StackExchangeRedis",
            "ChengYuan.DistributedLocking.Redis",
            "ChengYuan.ObjectMapping.Mapster");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(
            WebFacetMustNotReferencePersistence,
            ProviderFacetMustNotReferenceHosting,
            ModuleDependencyMustRespectCategoryOrder);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationAction(AnalyzeCompilation);
    }

    private static void AnalyzeCompilation(CompilationAnalysisContext context)
    {
        var assemblyName = context.Compilation.AssemblyName ?? string.Empty;
        if (assemblyName.Length == 0)
        {
            return;
        }

        var referencedAssemblyNames = context.Compilation.ReferencedAssemblyNames
            .Select(reference => reference.Name ?? string.Empty)
            .Where(referenceName => referenceName.Length != 0)
            .OrderBy(referenceName => referenceName, StringComparer.Ordinal)
            .ToArray();

        if (IsWebFacet(assemblyName))
        {
            ReportMatchingReferences(
                context,
                assemblyName,
                referencedAssemblyNames,
                referenceName => IsChengYuanAssembly(referenceName) &&
                    referenceName.EndsWith(".Persistence", StringComparison.Ordinal),
                WebFacetMustNotReferencePersistence);
        }

        if (IsProviderAssembly(assemblyName))
        {
            ReportMatchingReferences(
                context,
                assemblyName,
                referencedAssemblyNames,
                referenceName => referenceName == "ChengYuan.Hosting",
                ProviderFacetMustNotReferenceHosting);
        }

        if (assemblyName.EndsWith(".Application", StringComparison.Ordinal))
        {
            ReportMatchingReferences(
                context,
                assemblyName,
                referencedAssemblyNames,
                referenceName =>
                    IsChengYuanAssembly(referenceName) &&
                    (referenceName.EndsWith(".Persistence", StringComparison.Ordinal) ||
                     referenceName.EndsWith(".Web", StringComparison.Ordinal)),
                ModuleDependencyMustRespectCategoryOrder);
        }
    }

    private static void ReportMatchingReferences(
        CompilationAnalysisContext context,
        string assemblyName,
        string[] referencedAssemblyNames,
        Func<string, bool> predicate,
        DiagnosticDescriptor descriptor)
    {
        foreach (var referencedAssemblyName in referencedAssemblyNames.Where(predicate))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                descriptor,
                Location.None,
                assemblyName,
                referencedAssemblyName));
        }
    }

    private static bool IsWebFacet(string assemblyName)
        => assemblyName.EndsWith(".Web", StringComparison.Ordinal);

    private static bool IsChengYuanAssembly(string assemblyName)
        => assemblyName.StartsWith("ChengYuan.", StringComparison.Ordinal);

    private static bool IsProviderAssembly(string assemblyName)
        => ProviderAssemblyNames.Contains(assemblyName, StringComparer.Ordinal);

    private static DiagnosticDescriptor CreateDescriptor(string diagnosticId)
    {
        var rule = ArchitectureDiagnosticCatalog.Rules.Single(candidate => candidate.Id == diagnosticId);
        return new DiagnosticDescriptor(
            rule.Id,
            rule.Title,
            rule.Message,
            rule.Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: rule.Rationale,
            helpLinkUri: $"{HelpLinkBaseUri}/{rule.Id}");
    }
}
