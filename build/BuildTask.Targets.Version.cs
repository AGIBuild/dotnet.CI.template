using System;
using System.IO;
using System.Text.RegularExpressions;
using Nuke.Common;

partial class BuildTask
{
    const string VersionPrefixPattern = @"<VersionPrefix>\s*([^<]+?)\s*</VersionPrefix>";

    Target ShowVersion => _ => _
        .Executes(() =>
        {
            Console.WriteLine($"Current VersionPrefix: {ReadCurrentVersionPrefix()}");
        });

    Target UpdateVersion => _ => _
        .Executes(() =>
        {
            var current = ReadCurrentVersionPrefix();

            if (!string.IsNullOrWhiteSpace(VersionPrefix))
            {
                if (!Regex.IsMatch(VersionPrefix, @"^[0-9]+\.[0-9]+\.[0-9]+(-[0-9A-Za-z.-]+)?$"))
                    throw new ArgumentException($"Invalid VersionPrefix: '{VersionPrefix}'. Expected semver format like 1.2.3 or 1.2.3-rc.1.");
                WriteVersionPrefix(current, VersionPrefix);
                return;
            }

            var next = PatchIncrement(current);
            WriteVersionPrefix(current, next);
        });

    string ReadCurrentVersionPrefix()
    {
        var content = File.ReadAllText(DirectoryBuildPropsFile);
        var match = Regex.Match(content, VersionPrefixPattern, RegexOptions.Singleline);
        if (!match.Success)
            throw new InvalidOperationException($"<VersionPrefix> not found in {DirectoryBuildPropsFile}");
        return match.Groups[1].Value.Trim();
    }

    static string PatchIncrement(string current)
    {
        var baseVersion = current.Split('-')[0];
        var parts = baseVersion.Split('.');
        if (parts.Length < 3 || !int.TryParse(parts[0], out var major) || !int.TryParse(parts[1], out var minor) || !int.TryParse(parts[2], out var patch))
            throw new InvalidOperationException($"Cannot parse current version '{current}' as X.Y.Z.");

        return $"{major}.{minor}.{patch + 1}";
    }

    void WriteVersionPrefix(string current, string next)
    {
        if (string.Equals(current, next, StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"VersionPrefix is already {next}.");
            return;
        }

        var content = File.ReadAllText(DirectoryBuildPropsFile);
        var updated = Regex.Replace(
            content,
            VersionPrefixPattern,
            $"<VersionPrefix>{next}</VersionPrefix>",
            RegexOptions.Singleline);

        File.WriteAllText(DirectoryBuildPropsFile, updated);
        Console.WriteLine($"Updated VersionPrefix: {current} -> {next}");
    }
}
