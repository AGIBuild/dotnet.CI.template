using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using ChengYuan.ArchitectureAnalyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ChengYuan.ArchitectureAnalyzer.Tests;

internal static class AnalyzerTestHarness
{
    private static readonly CSharpParseOptions ParseOptions =
        CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);

    private static readonly CSharpCompilationOptions CompilationOptions =
        new(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable);

    private static readonly ImmutableArray<MetadataReference> FrameworkReferences =
        CreateFrameworkReferences();

    private static readonly ConcurrentDictionary<string, MetadataReference> ReferenceAssemblyCache = new(StringComparer.Ordinal);

    public static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(
        string assemblyName,
        string source,
        params string[] referencedAssemblyNames)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, ParseOptions);
        var references = FrameworkReferences.AddRange(
            referencedAssemblyNames.Select(CreateReferenceAssembly));

        var compilation = CSharpCompilation.Create(
            assemblyName,
            [syntaxTree],
            references,
            CompilationOptions);

        var compilationWithAnalyzers = compilation.WithAnalyzers(
            ImmutableArray.Create<DiagnosticAnalyzer>(new ChengYuanArchitectureAnalyzer()));

        return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
    }

    public static Task<ImmutableArray<Diagnostic>> AnalyzeReferencesAsync(
        string assemblyName,
        params string[] referencedAssemblyNames)
        => AnalyzeAsync(
            assemblyName,
            "namespace AnalyzerReferenceBoundarySample; public sealed class Marker;",
            referencedAssemblyNames);

    private static MetadataReference CreateReferenceAssembly(string assemblyName)
        => ReferenceAssemblyCache.GetOrAdd(
            assemblyName,
            static name =>
            {
                var compilation = CSharpCompilation.Create(
                    name,
                    [CSharpSyntaxTree.ParseText("namespace ReferenceAssemblySample; public sealed class Marker;", ParseOptions)],
                    FrameworkReferences,
                    CompilationOptions);

                using var stream = new MemoryStream();
                var result = compilation.Emit(stream);
                if (!result.Success)
                {
                    var errors = string.Join(Environment.NewLine, result.Diagnostics);
                    throw new InvalidOperationException($"Failed to create reference assembly '{name}':{Environment.NewLine}{errors}");
                }

                return MetadataReference.CreateFromImage(stream.ToArray());
            });

    private static ImmutableArray<MetadataReference> CreateFrameworkReferences()
    {
        var trustedPlatformAssemblies = (string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
        if (trustedPlatformAssemblies is null)
        {
            return ImmutableArray.Create<MetadataReference>(
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location));
        }

        return trustedPlatformAssemblies
            .Split(Path.PathSeparator)
            .Where(static path => !Path.GetFileNameWithoutExtension(path).StartsWith("ChengYuan.", StringComparison.Ordinal))
            .Select(static path => MetadataReference.CreateFromFile(path))
            .ToImmutableArray<MetadataReference>();
    }
}
