using System;
using System.IO;
using System.Text.RegularExpressions;
using Nuke.Common;

partial class BuildTask
{
    Target UpdateVersion => _ => _
        .Requires(() => !string.IsNullOrWhiteSpace(VersionPrefix))
        .Executes(() =>
        {
            const string semverPattern = "^[0-9]+\\.[0-9]+\\.[0-9]+(-[0-9A-Za-z.-]+)?$";
            if (!Regex.IsMatch(VersionPrefix, semverPattern))
                throw new ArgumentException($"Invalid VersionPrefix: '{VersionPrefix}'. Expected semver format like 1.2.3 or 1.2.3-rc.1.");

            if (!File.Exists(DirectoryBuildPropsFile))
                throw new FileNotFoundException($"File not found: {DirectoryBuildPropsFile}");

            var content = File.ReadAllText(DirectoryBuildPropsFile);
            const string versionPrefixNodePattern = "<VersionPrefix>\\s*([^<]+?)\\s*</VersionPrefix>";
            var match = Regex.Match(content, versionPrefixNodePattern, RegexOptions.Singleline);
            if (!match.Success)
                throw new InvalidOperationException($"<VersionPrefix> element not found in {DirectoryBuildPropsFile}");

            var current = match.Groups[1].Value.Trim();
            if (string.Equals(current, VersionPrefix, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"VersionPrefix is already {VersionPrefix}.");
                return;
            }

            var updated = Regex.Replace(
                content,
                versionPrefixNodePattern,
                $"<VersionPrefix>{VersionPrefix}</VersionPrefix>",
                RegexOptions.Singleline);

            File.WriteAllText(DirectoryBuildPropsFile, updated);
            Console.WriteLine($"Updated VersionPrefix: {current} -> {VersionPrefix}");
        });
}
