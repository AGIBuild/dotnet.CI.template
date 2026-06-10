using Shouldly;

namespace ChengYuan.ArchitectureAnalyzer.Tests;

public sealed class ReferenceBoundaryAnalyzerTests
{
    [Fact]
    public async Task WebFacetReferencingPersistenceReportsDiagnostic()
    {
        var diagnostics = await AnalyzerTestHarness.AnalyzeReferencesAsync(
            "ChengYuan.Identity.Web",
            "ChengYuan.Identity.Persistence");

        diagnostics.Select(static diagnostic => diagnostic.Id)
            .ShouldContain("CYARCH001");
    }

    [Fact]
    public async Task WebFacetReferencingApplicationDoesNotReportPersistenceDiagnostic()
    {
        var diagnostics = await AnalyzerTestHarness.AnalyzeReferencesAsync(
            "ChengYuan.Identity.Web",
            "ChengYuan.Identity.Application");

        diagnostics.Select(static diagnostic => diagnostic.Id)
            .ShouldNotContain("CYARCH001");
    }

    [Fact]
    public async Task ProviderReferencingHostingReportsDiagnostic()
    {
        var diagnostics = await AnalyzerTestHarness.AnalyzeReferencesAsync(
            "ChengYuan.Data.Sqlite",
            "ChengYuan.Hosting");

        diagnostics.Select(static diagnostic => diagnostic.Id)
            .ShouldContain("CYARCH002");
    }

    [Fact]
    public async Task ApplicationReferencingWebFacetReportsLayeringDiagnostic()
    {
        var diagnostics = await AnalyzerTestHarness.AnalyzeReferencesAsync(
            "ChengYuan.Identity.Application",
            "ChengYuan.Identity.Web");

        diagnostics.Select(static diagnostic => diagnostic.Id)
            .ShouldContain("CYARCH004");
    }

    [Fact]
    public async Task ContractsReferencingAnotherModulePersistenceFacetReportsDiagnostic()
    {
        var diagnostics = await AnalyzerTestHarness.AnalyzeReferencesAsync(
            "ChengYuan.PermissionManagement.Contracts",
            "ChengYuan.Identity.Persistence");

        diagnostics.Select(static diagnostic => diagnostic.Id)
            .ShouldContain("CYARCH008");
    }

    [Fact]
    public async Task DomainReferencingAnotherModuleWebFacetReportsDiagnostic()
    {
        var diagnostics = await AnalyzerTestHarness.AnalyzeReferencesAsync(
            "ChengYuan.Identity.Domain",
            "ChengYuan.PermissionManagement.Web");

        diagnostics.Select(static diagnostic => diagnostic.Id)
            .ShouldContain("CYARCH008");
    }

    [Fact]
    public async Task HostReferencingPersistenceFacetDoesNotReportCrossFacetDiagnostic()
    {
        var diagnostics = await AnalyzerTestHarness.AnalyzeReferencesAsync(
            "ChengYuan.WebHost",
            "ChengYuan.Identity.Persistence");

        diagnostics.Select(static diagnostic => diagnostic.Id)
            .ShouldNotContain("CYARCH008");
    }

    [Fact]
    public async Task TestAssemblyReferencingFacetsDoesNotReportCrossFacetDiagnostic()
    {
        var diagnostics = await AnalyzerTestHarness.AnalyzeReferencesAsync(
            "ChengYuan.FrameworkKernel.Tests",
            "ChengYuan.Identity.Persistence",
            "ChengYuan.PermissionManagement.Web");

        diagnostics.Select(static diagnostic => diagnostic.Id)
            .ShouldNotContain("CYARCH008");
    }

    [Fact]
    public async Task SameModulePersistenceReferencingApplicationDoesNotReportCrossFacetDiagnostic()
    {
        var diagnostics = await AnalyzerTestHarness.AnalyzeReferencesAsync(
            "ChengYuan.Identity.Persistence",
            "ChengYuan.Identity.Application");

        diagnostics.Select(static diagnostic => diagnostic.Id)
            .ShouldNotContain("CYARCH008");
    }

    [Fact]
    public async Task CoreReferencingAnotherChengYuanAssemblyReportsDiagnostic()
    {
        var diagnostics = await AnalyzerTestHarness.AnalyzeReferencesAsync(
            "ChengYuan.Core",
            "ChengYuan.ExecutionContext");

        diagnostics.Select(static diagnostic => diagnostic.Id)
            .ShouldContain("CYARCH007");
    }

    [Fact]
    public async Task CoreReferencingExternalAssemblyDoesNotReportDiagnostic()
    {
        var diagnostics = await AnalyzerTestHarness.AnalyzeReferencesAsync(
            "ChengYuan.Core",
            "Microsoft.Extensions.Logging.Abstractions");

        diagnostics.Select(static diagnostic => diagnostic.Id)
            .ShouldNotContain("CYARCH007");
    }
}
