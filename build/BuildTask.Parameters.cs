using Nuke.Common;
using Nuke.Common.IO;

partial class BuildTask
{
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

    [Parameter("Minimum line coverage percentage (0-100). CoverageReport fails if below this threshold.")]
    readonly int CoverageThreshold = 90;

    [Parameter("Minimum branch coverage percentage (0-100). CoverageReport fails if below this threshold.")]
    readonly int BranchCoverageThreshold = 90;

    // Common output/input locations.
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    AbsolutePath TestResultsDirectory => ArtifactsDirectory / "test-results";
    AbsolutePath PackagesDirectory => ArtifactsDirectory / "packages";
    AbsolutePath PublishDirectory => ArtifactsDirectory / "publish";
    AbsolutePath InstallersDirectory => ArtifactsDirectory / "installers";

    AbsolutePath ReleaseManifestFile => PackagesDirectory / "release-manifest.json";
    AbsolutePath BuildOutputsMarkerFile => ArtifactsDirectory / ".build-outputs" / "build-outputs.json";
    AbsolutePath ToolManifestFile => RootDirectory / ".config" / "dotnet-tools.json";
    AbsolutePath DirectoryBuildPropsFile => RootDirectory / "Directory.Build.props";
}
