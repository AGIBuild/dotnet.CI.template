using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using Nuke.Common;

partial class BuildTask
{
    Target GenerateReleaseManifest => _ => _
        .DependsOn(Pack)
        .Executes(() =>
        {
            var packageFiles = Directory.GetFiles(PackagesDirectory, "*.*", SearchOption.TopDirectoryOnly)
                .Where(f => f.EndsWith(".nupkg", StringComparison.OrdinalIgnoreCase)
                         || f.EndsWith(".snupkg", StringComparison.OrdinalIgnoreCase))
                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (packageFiles.Count == 0)
                throw new InvalidOperationException($"No .nupkg/.snupkg files found in {PackagesDirectory}");

            var version = ReadCurrentVersionPrefix();
            var commitSha = GetGitHeadSha();

            var packages = packageFiles.Select(f => new Dictionary<string, string>
            {
                ["file"] = Path.GetFileName(f),
                ["sha256"] = ComputeSha256(f)
            }).ToList();

            var manifest = new Dictionary<string, object>
            {
                ["schemaVersion"] = 1,
                ["version"] = version,
                ["commitSha"] = commitSha,
                ["packages"] = packages
            };

            var json = JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ReleaseManifestFile, json);
            Console.WriteLine($"Release manifest written to {ReleaseManifestFile}");
            Console.WriteLine($"  version={version}  commitSha={commitSha}  packages={packages.Count}");
        });

    static string GetGitHeadSha()
    {
        var psi = new ProcessStartInfo("git", "rev-parse HEAD")
        {
            RedirectStandardOutput = true,
            UseShellExecute = false
        };
        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start git");
        var sha = process.StandardOutput.ReadToEnd().Trim();
        process.WaitForExit();
        if (process.ExitCode != 0 || string.IsNullOrWhiteSpace(sha))
            throw new InvalidOperationException("Failed to read git HEAD SHA");
        return sha;
    }

    static string ComputeSha256(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        var hash = SHA256.HashData(stream);
        return Convert.ToHexStringLower(hash);
    }
}
