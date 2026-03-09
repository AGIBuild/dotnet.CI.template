using System.IO;
using System.Text.Json;

partial class BuildTask
{
    sealed record BuildOutputsMarker(string BuildPath, string Configuration, string VersionSuffix);

    BuildOutputsMarker? TryReadBuildOutputsMarker()
    {
        if (!File.Exists(BuildOutputsMarkerFile))
            return null;

        var json = File.ReadAllText(BuildOutputsMarkerFile);
        return JsonSerializer.Deserialize<BuildOutputsMarker>(json)
            ?? throw new IOException($"Failed to parse build outputs marker: {BuildOutputsMarkerFile}");
    }

    void WriteBuildOutputsMarker()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(BuildOutputsMarkerFile)!);
        var marker = new BuildOutputsMarker(BuildPath, Configuration.ToString(), VersionSuffix ?? string.Empty);
        File.WriteAllText(BuildOutputsMarkerFile, JsonSerializer.Serialize(marker));
    }

    bool CanSkipBuild(string projectFile)
    {
        if (!string.Equals(BuildPath, projectFile, System.StringComparison.OrdinalIgnoreCase))
            return false;

        var marker = TryReadBuildOutputsMarker();
        if (marker is null)
            return false;

        return string.Equals(marker.BuildPath, BuildPath, System.StringComparison.OrdinalIgnoreCase)
            && string.Equals(marker.Configuration, Configuration.ToString(), System.StringComparison.OrdinalIgnoreCase)
            && string.Equals(marker.VersionSuffix, VersionSuffix ?? string.Empty, System.StringComparison.OrdinalIgnoreCase);
    }
}
