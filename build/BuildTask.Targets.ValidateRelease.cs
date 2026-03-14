using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using Nuke.Common;

partial class BuildTask
{
    Target ValidatePackageVersions => _ => _
        .DependsOn(Pack)
        .Executes(() =>
        {
            var expectedVersion = ReadCurrentVersionPrefix();
            if (!string.IsNullOrWhiteSpace(VersionSuffix))
                expectedVersion = $"{expectedVersion}-{VersionSuffix}";

            var packageFiles = Directory.GetFiles(PackagesDirectory, "*.*", SearchOption.TopDirectoryOnly)
                .Where(f => f.EndsWith(".nupkg", StringComparison.OrdinalIgnoreCase)
                         || f.EndsWith(".snupkg", StringComparison.OrdinalIgnoreCase))
                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (packageFiles.Count == 0)
                throw new InvalidOperationException($"No package files found under {PackagesDirectory}");

            var errors = new List<string>();
            foreach (var file in packageFiles)
            {
                var name = Path.GetFileName(file);
                if (!name.Contains($".{expectedVersion}.nupkg", StringComparison.OrdinalIgnoreCase) &&
                    !name.Contains($".{expectedVersion}.snupkg", StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add($"Package version mismatch. Expected {expectedVersion}, got {name}");
                }
            }

            if (errors.Count > 0)
            {
                foreach (var error in errors)
                    Console.Error.WriteLine($"ERROR: {error}");
                throw new InvalidOperationException(
                    $"Package version validation failed with {errors.Count} error(s).");
            }

            Console.WriteLine($"All {packageFiles.Count} packages match version: {expectedVersion}");
        });

    Target ValidateReleaseManifest => _ => _
        .Executes(() =>
        {
            if (!File.Exists(ReleaseManifestFile))
                throw new FileNotFoundException($"Release manifest not found: {ReleaseManifestFile}");

            var expectedVersion = ReadCurrentVersionPrefix();

            using var doc = JsonDocument.Parse(File.ReadAllText(ReleaseManifestFile));
            var root = doc.RootElement;

            var manifestVersion = root.GetProperty("version").GetString();
            if (manifestVersion != expectedVersion)
                throw new InvalidOperationException(
                    $"Version mismatch: manifest={manifestVersion} expected={expectedVersion}");

            var packages = root.GetProperty("packages");
            if (packages.GetArrayLength() == 0)
                throw new InvalidOperationException(
                    "Release manifest contains an empty packages array. At least one package is required.");

            var errors = new List<string>();

            foreach (var pkg in packages.EnumerateArray())
            {
                var fileName = pkg.GetProperty("file").GetString()!;
                var expectedHash = pkg.GetProperty("sha256").GetString()!;
                var filePath = PackagesDirectory / fileName;

                if (!File.Exists(filePath))
                {
                    errors.Add($"Missing: {filePath}");
                    continue;
                }

                using var stream = File.OpenRead(filePath);
                var actualHash = Convert.ToHexStringLower(SHA256.HashData(stream));
                if (actualHash != expectedHash)
                    errors.Add($"Hash mismatch: {fileName}");
            }

            if (errors.Count > 0)
            {
                foreach (var error in errors)
                    Console.Error.WriteLine($"ERROR: {error}");
                throw new InvalidOperationException(
                    $"Release manifest validation failed with {errors.Count} error(s).");
            }

            Console.WriteLine($"Verified {packages.GetArrayLength()} packages.");
        });
}
