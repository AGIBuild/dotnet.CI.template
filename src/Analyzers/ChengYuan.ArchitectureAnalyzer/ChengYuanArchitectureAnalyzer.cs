using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

    private static readonly DiagnosticDescriptor HostMustOwnRuntimePersistenceComposition = CreateDescriptor(
        ArchitectureDiagnosticIds.HostMustOwnRuntimePersistenceComposition);

    private static readonly DiagnosticDescriptor HostDiagnosticsMustBeExplicitlyEnabled = CreateDescriptor(
        ArchitectureDiagnosticIds.HostDiagnosticsMustBeExplicitlyEnabled);

    private static readonly DiagnosticDescriptor CoreMustRemainFoundationOnly = CreateDescriptor(
        ArchitectureDiagnosticIds.CoreMustRemainFoundationOnly);

    private static readonly DiagnosticDescriptor ModuleMustNotReferenceForeignFacet = CreateDescriptor(
        ArchitectureDiagnosticIds.ModuleMustNotReferenceForeignFacet);

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
            ModuleDependencyMustRespectCategoryOrder,
            HostMustOwnRuntimePersistenceComposition,
            HostDiagnosticsMustBeExplicitlyEnabled,
            CoreMustRemainFoundationOnly,
            ModuleMustNotReferenceForeignFacet);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationAction(AnalyzeCompilation);
        context.RegisterSymbolAction(AnalyzeModuleType, SymbolKind.NamedType);
        context.RegisterSyntaxNodeAction(AnalyzeDiagnosticEndpointInvocation, SyntaxKind.InvocationExpression);
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

        if (assemblyName == "ChengYuan.Core")
        {
            ReportMatchingReferences(
                context,
                assemblyName,
                referencedAssemblyNames,
                IsChengYuanAssembly,
                CoreMustRemainFoundationOnly);
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

        if (!IsHostAssembly(assemblyName) && !IsTestAssembly(assemblyName))
        {
            ReportMatchingReferences(
                context,
                assemblyName,
                referencedAssemblyNames,
                referenceName => IsForeignFacetReference(assemblyName, referenceName),
                ModuleMustNotReferenceForeignFacet);
        }
    }

    private static void AnalyzeModuleType(SymbolAnalysisContext context)
    {
        if (context.Symbol is not INamedTypeSymbol moduleType ||
            !IsWebModule(moduleType))
        {
            return;
        }

        foreach (var attribute in moduleType.GetAttributes().Where(IsDependsOnAttribute))
        {
            foreach (var dependencyType in GetDependsOnModuleTypes(attribute))
            {
                if (!IsPersistenceModule(dependencyType))
                {
                    continue;
                }

                context.ReportDiagnostic(Diagnostic.Create(
                    HostMustOwnRuntimePersistenceComposition,
                    GetAttributeLocation(attribute, moduleType),
                    dependencyType.Name,
                    moduleType.Name));
            }
        }
    }

    private static void AnalyzeDiagnosticEndpointInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        var methodName = GetInvokedMethodName(invocation);
        if (methodName is null)
        {
            return;
        }

        if (methodName == "MapOpenApi")
        {
            ReportIfNotExplicitlyGated(context, invocation, methodName);
            return;
        }

        if (methodName == "MapGet" && IsDetailedHealthEndpoint(invocation))
        {
            ReportIfNotExplicitlyGated(context, invocation, "MapGet(\"/health\")");
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

    private static bool IsHostAssembly(string assemblyName)
        => assemblyName.EndsWith("Host", StringComparison.Ordinal);

    private static bool IsTestAssembly(string assemblyName)
        => assemblyName.EndsWith(".Tests", StringComparison.Ordinal);

    private static bool IsForeignFacetReference(string assemblyName, string referencedAssemblyName)
        => IsChengYuanAssembly(referencedAssemblyName) &&
            IsTransportOrPersistenceFacet(referencedAssemblyName) &&
            GetModuleIdentity(assemblyName) != GetModuleIdentity(referencedAssemblyName);

    private static bool IsTransportOrPersistenceFacet(string assemblyName)
        => assemblyName.EndsWith(".Web", StringComparison.Ordinal) ||
            assemblyName.EndsWith(".Persistence", StringComparison.Ordinal) ||
            assemblyName.EndsWith(".Cli", StringComparison.Ordinal);

    private static string GetModuleIdentity(string assemblyName)
    {
        foreach (var suffix in new[] { ".Application", ".Contracts", ".Domain", ".Web", ".Persistence", ".Cli" })
        {
            if (assemblyName.EndsWith(suffix, StringComparison.Ordinal))
            {
                return assemblyName.Substring(0, assemblyName.Length - suffix.Length);
            }
        }

        return assemblyName;
    }

    private static bool IsDependsOnAttribute(AttributeData attribute)
        => attribute.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ==
            "global::ChengYuan.Core.Modularity.DependsOnAttribute";

    private static ImmutableArray<INamedTypeSymbol> GetDependsOnModuleTypes(AttributeData attribute)
    {
        var builder = ImmutableArray.CreateBuilder<INamedTypeSymbol>();

        foreach (var argument in attribute.ConstructorArguments)
        {
            AddDependsOnModuleTypes(builder, argument);
        }

        return builder.ToImmutable();
    }

    private static void AddDependsOnModuleTypes(
        ImmutableArray<INamedTypeSymbol>.Builder builder,
        TypedConstant argument)
    {
        if (argument.Kind == TypedConstantKind.Type && argument.Value is INamedTypeSymbol moduleType)
        {
            builder.Add(moduleType);
            return;
        }

        if (argument.Kind != TypedConstantKind.Array)
        {
            return;
        }

        foreach (var value in argument.Values)
        {
            AddDependsOnModuleTypes(builder, value);
        }
    }

    private static bool IsWebModule(INamedTypeSymbol type)
        => type.Name.EndsWith("WebModule", StringComparison.Ordinal) &&
            InheritsFrom(type, "ChengYuan.Core.Modularity.ExtensionModule");

    private static bool IsPersistenceModule(INamedTypeSymbol type)
        => type.Name.EndsWith("PersistenceModule", StringComparison.Ordinal) ||
            type.ContainingAssembly.Name.EndsWith(".Persistence", StringComparison.Ordinal);

    private static bool InheritsFrom(INamedTypeSymbol type, string metadataName)
    {
        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (current.ToDisplayString() == metadataName)
            {
                return true;
            }
        }

        return false;
    }

    private static Location GetAttributeLocation(AttributeData attribute, INamedTypeSymbol fallbackSymbol)
        => attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation() ??
            fallbackSymbol.Locations.FirstOrDefault() ??
            Location.None;

    private static string? GetInvokedMethodName(InvocationExpressionSyntax invocation)
        => invocation.Expression switch
        {
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.ValueText,
            IdentifierNameSyntax identifierName => identifierName.Identifier.ValueText,
            _ => null
        };

    private static bool IsDetailedHealthEndpoint(InvocationExpressionSyntax invocation)
    {
        var arguments = invocation.ArgumentList.Arguments;
        if (arguments.Count == 0 ||
            arguments[0].Expression is not LiteralExpressionSyntax routeLiteral ||
            routeLiteral.Token.ValueText != "/health")
        {
            return false;
        }

        var invocationText = invocation.ToString();
        return ContainsOrdinal(invocationText, "IModuleCatalog") ||
            ContainsOrdinal(invocationText, "ModuleTypes") ||
            ContainsOrdinal(invocationText, "modules");
    }

    private static void ReportIfNotExplicitlyGated(
        SyntaxNodeAnalysisContext context,
        InvocationExpressionSyntax invocation,
        string mappingName)
    {
        if (IsGatedByEnvironmentOrConfiguration(invocation))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            HostDiagnosticsMustBeExplicitlyEnabled,
            invocation.GetLocation(),
            mappingName));
    }

    private static bool IsGatedByEnvironmentOrConfiguration(SyntaxNode node)
    {
        foreach (var ifStatement in node.Ancestors().OfType<IfStatementSyntax>())
        {
            var condition = ifStatement.Condition.ToString();
            if (ContainsOrdinal(condition, "IsDevelopment") ||
                ContainsOrdinal(condition, "Environment") ||
                ContainsOrdinal(condition, "Configuration") ||
                ContainsOrdinal(condition, "Enabled"))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsOrdinal(string value, string searchValue)
        => value.IndexOf(searchValue, StringComparison.Ordinal) >= 0;

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
