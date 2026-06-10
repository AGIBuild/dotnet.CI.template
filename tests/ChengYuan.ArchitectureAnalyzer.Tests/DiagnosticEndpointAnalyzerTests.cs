using Shouldly;

namespace ChengYuan.ArchitectureAnalyzer.Tests;

public sealed class DiagnosticEndpointAnalyzerTests
{
    [Fact]
    public async Task UngatedOpenApiMappingReportsDiagnostic()
    {
        var diagnostics = await AnalyzerTestHarness.AnalyzeAsync(
            "ChengYuan.WebHost",
            DiagnosticEndpointStubs(
                """
                public static void Configure(WebApplication app)
                {
                    app.MapOpenApi();
                }
                """));

        diagnostics.Select(static diagnostic => diagnostic.Id)
            .ShouldContain("CYARCH006");
    }

    [Fact]
    public async Task EnvironmentGatedOpenApiMappingDoesNotReportDiagnostic()
    {
        var diagnostics = await AnalyzerTestHarness.AnalyzeAsync(
            "ChengYuan.WebHost",
            DiagnosticEndpointStubs(
                """
                public static void Configure(WebApplication app)
                {
                    if (app.Environment.IsDevelopment())
                    {
                        app.MapOpenApi();
                    }
                }
                """));

        diagnostics.Select(static diagnostic => diagnostic.Id)
            .ShouldNotContain("CYARCH006");
    }

    [Fact]
    public async Task UngatedDetailedHealthEndpointReportsDiagnostic()
    {
        var diagnostics = await AnalyzerTestHarness.AnalyzeAsync(
            "ChengYuan.WebHost",
            DiagnosticEndpointStubs(
                """
                public static void Configure(WebApplication app)
                {
                    app.MapGet("/health", () => new { modules = new[] { "Core" } });
                }
                """));

        diagnostics.Select(static diagnostic => diagnostic.Id)
            .ShouldContain("CYARCH006");
    }

    [Fact]
    public async Task SimpleHealthEndpointDoesNotReportDiagnostic()
    {
        var diagnostics = await AnalyzerTestHarness.AnalyzeAsync(
            "ChengYuan.WebHost",
            DiagnosticEndpointStubs(
                """
                public static void Configure(WebApplication app)
                {
                    app.MapGet("/health", () => "OK");
                }
                """));

        diagnostics.Select(static diagnostic => diagnostic.Id)
            .ShouldNotContain("CYARCH006");
    }

    private static string DiagnosticEndpointStubs(string configureMethod)
        => $$"""
        using System;

        namespace Microsoft.AspNetCore.Builder
        {
            public sealed class WebApplication
            {
                public WebHostEnvironment Environment { get; } = new();
            }

            public sealed class WebHostEnvironment
            {
                public bool IsDevelopment() => true;
            }

            public static class EndpointRouteBuilderExtensions
            {
                public static void MapOpenApi(this WebApplication app)
                {
                }

                public static void MapGet(this WebApplication app, string route, Func<object> handler)
                {
                }
            }
        }

        namespace ChengYuan.WebHost
        {
            using Microsoft.AspNetCore.Builder;

            public static class WebHostApplicationExtensions
            {
                {{configureMethod}}
            }
        }
        """;
}
