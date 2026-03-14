using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class BuildTask
{
    Target CoverageReport => _ => _
        .DependsOn(Test)
        .Executes(() =>
        {
            var coverageFiles = TestResultsDirectory.GlobFiles("**/coverage.cobertura.xml");
            if (!coverageFiles.Any())
            {
                Console.WriteLine("No coverage files found. Skipping report generation.");
                return;
            }

            var reportDir = TestResultsDirectory / "coverage-report";
            var reports = string.Join(";", coverageFiles);

            DotNet(
                $"reportgenerator " +
                $"-reports:{reports} " +
                $"-targetdir:{reportDir} " +
                $"-reporttypes:Cobertura;HtmlSummary " +
                $"-assemblyfilters:-*.Tests",
                workingDirectory: RootDirectory);

            Console.WriteLine($"Coverage report generated at: {reportDir}");
        });
}
