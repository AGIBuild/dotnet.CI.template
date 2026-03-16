using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nuke.Common;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class BuildTask
{
    Target PushNuGetPackages => _ => _
        .Executes(() =>
        {
            var apiKey = string.IsNullOrWhiteSpace(NuGetApiKey)
                ? EnvironmentInfo.GetVariable<string>("NUGET_API_KEY")
                : NuGetApiKey;

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException(
                    "NuGet API key is required. Use --NuGetApiKey or NUGET_API_KEY environment variable.");

            var packageFiles = Directory.GetFiles(PackagesDirectory, "*.nupkg", SearchOption.TopDirectoryOnly)
                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (packageFiles.Count == 0)
                throw new InvalidOperationException($"No .nupkg files found under {PackagesDirectory}");

            var errors = new List<string>();
            foreach (var packageFile in packageFiles)
            {
                try
                {
                    Console.WriteLine($"Pushing {Path.GetFileName(packageFile)} to NuGet.org");
                    DotNet($"nuget push \"{packageFile}\" --api-key \"{apiKey}\" --source https://api.nuget.org/v3/index.json --skip-duplicate",
                        workingDirectory: RootDirectory);
                }
                catch (Exception ex)
                {
                    errors.Add($"{Path.GetFileName(packageFile)}: {ex.Message}");
                }
            }

            if (errors.Count > 0)
            {
                foreach (var error in errors)
                    Console.Error.WriteLine($"ERROR: {error}");

                throw new InvalidOperationException(
                    $"NuGet push failed for {errors.Count} package(s). Review errors above.");
            }

            Console.WriteLine($"NuGet push completed for {packageFiles.Count} package(s).");
        });
}
