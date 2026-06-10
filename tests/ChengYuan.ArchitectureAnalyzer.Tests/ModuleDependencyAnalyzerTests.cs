using Shouldly;

namespace ChengYuan.ArchitectureAnalyzer.Tests;

public sealed class ModuleDependencyAnalyzerTests
{
    [Fact]
    public async Task WebModuleDependingOnPersistenceModuleReportsDiagnostic()
    {
        var diagnostics = await AnalyzerTestHarness.AnalyzeAsync(
            "ChengYuan.Identity.Web",
            """
            using System;

            namespace ChengYuan.Core.Modularity
            {
                [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
                public sealed class DependsOnAttribute(params Type[] moduleTypes) : Attribute;

                public abstract class ExtensionModule;
            }

            namespace ChengYuan.Identity
            {
                using ChengYuan.Core.Modularity;

                public sealed class IdentityPersistenceModule : ExtensionModule;

                [DependsOn(typeof(IdentityPersistenceModule))]
                public sealed class IdentityWebModule : ExtensionModule;
            }
            """);

        diagnostics.Select(static diagnostic => diagnostic.Id)
            .ShouldContain("CYARCH003");
    }

    [Fact]
    public async Task WebModuleDependingOnApplicationModuleDoesNotReportDiagnostic()
    {
        var diagnostics = await AnalyzerTestHarness.AnalyzeAsync(
            "ChengYuan.Identity.Web",
            """
            using System;

            namespace ChengYuan.Core.Modularity
            {
                [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
                public sealed class DependsOnAttribute(params Type[] moduleTypes) : Attribute;

                public abstract class ExtensionModule;
            }

            namespace ChengYuan.Identity
            {
                using ChengYuan.Core.Modularity;

                public sealed class IdentityModule : ExtensionModule;

                [DependsOn(typeof(IdentityModule))]
                public sealed class IdentityWebModule : ExtensionModule;
            }
            """);

        diagnostics.Select(static diagnostic => diagnostic.Id)
            .ShouldNotContain("CYARCH003");
    }
}
