using Nuke.Common;
using Nuke.Common.IO;

partial class BuildTask
{
    // Stable external build interface (allowed CLI parameters):
    //   - Configuration
    //   - VersionPrefix  (used only by UpdateVersion target)
    //   - VersionSuffix  (prerelease suffix, e.g., "ci.158")
    //   - Runtime
    //   - SelfContained
    // VersionPrefix is owned by Directory.Build.props (single source of truth).

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Version prefix used by UpdateVersion target (e.g., 1.2.3)")]
    readonly string VersionPrefix = string.Empty;

    [Parameter("Version suffix for prerelease builds (e.g., ci.158)")]
    readonly string VersionSuffix = string.Empty;

    // Internal path conventions controlled by build tasks (not external parameters).
    readonly string BuildPath = "Dotnet.CI.Template.slnx";
    readonly string TestPath = "Dotnet.CI.Template.slnx";
    readonly string PackPath = "Dotnet.CI.Template.slnx";
    readonly string PublishPath = "src/Dotnet.CI.Template.Sample/Dotnet.CI.Template.Sample.csproj";

    // Publish inputs.
    [Parameter("Target runtime identifier used by Publish target")]
    readonly string Runtime = string.Empty;

    [Parameter("Publish self-contained output in Publish target")]
    readonly bool SelfContained;

    // Common output/input locations.
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    AbsolutePath TestResultsDirectory => ArtifactsDirectory / "test-results";
    AbsolutePath PackagesDirectory => ArtifactsDirectory / "packages";
    AbsolutePath PublishDirectory => ArtifactsDirectory / "publish";

    AbsolutePath BuildOutputsMarkerFile => ArtifactsDirectory / ".build-outputs" / "build-outputs.json";
    AbsolutePath ToolManifestFile => RootDirectory / ".config" / "dotnet-tools.json";
    AbsolutePath DirectoryBuildPropsFile => RootDirectory / "Directory.Build.props";
}
