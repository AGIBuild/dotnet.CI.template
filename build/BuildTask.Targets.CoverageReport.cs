using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
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
                $"-reporttypes:Cobertura;Html " +
                $"-assemblyfilters:-*.Tests " +
                $"-riskhotspotassemblyfilters:-*.Tests",
                workingDirectory: RootDirectory);

            Console.WriteLine($"Coverage report generated at: {reportDir}");

            EnforceCoverageThreshold(reportDir / "Cobertura.xml");
        });

    void EnforceCoverageThreshold(AbsolutePath coberturaFile)
    {
        if (!File.Exists(coberturaFile))
        {
            Console.WriteLine($"Merged Cobertura file not found at {coberturaFile}. Skipping threshold check.");
            return;
        }

        var doc = XDocument.Load(coberturaFile);
        var root = doc.Root;
        var errors = new List<string>();

        var lineRate = ParseRate(root, "line-rate");
        var branchRate = ParseRate(root, "branch-rate");

        if (lineRate.HasValue)
        {
            var pct = lineRate.Value * 100;
            Console.WriteLine($"Line coverage:   {pct:F1}% (threshold: {CoverageThreshold}%)");
            if (pct < CoverageThreshold)
                errors.Add($"Line coverage {pct:F1}% is below the minimum threshold of {CoverageThreshold}%.");
        }

        if (branchRate.HasValue)
        {
            var pct = branchRate.Value * 100;
            Console.WriteLine($"Branch coverage: {pct:F1}% (threshold: {BranchCoverageThreshold}%)");
            if (pct < BranchCoverageThreshold)
                errors.Add($"Branch coverage {pct:F1}% is below the minimum threshold of {BranchCoverageThreshold}%.");
        }

        if (errors.Count > 0)
        {
            foreach (var error in errors)
                Console.Error.WriteLine($"ERROR: {error}");
            throw new InvalidOperationException(
                string.Join(" ", errors) + " Review the coverage report for details.");
        }
    }

    static double? ParseRate(XElement? element, string attributeName)
    {
        var attr = element?.Attribute(attributeName);
        if (attr is not null &&
            double.TryParse(attr.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
        {
            return value;
        }

        Console.WriteLine($"Could not parse {attributeName} from Cobertura report. Skipping this metric.");
        return null;
    }
}
