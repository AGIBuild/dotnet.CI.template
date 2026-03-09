using Nuke.Common;

// Build entrypoint.
// VersionPrefix is owned by Directory.Build.props (single source of truth).
// Workflows pass VersionSuffix for prerelease builds; release builds omit it.
// Allowed CLI parameters: Configuration, VersionSuffix, Runtime, SelfContained.
partial class BuildTask : NukeBuild
{
    public static int Main() => Execute<BuildTask>(x => x.Build);
}
