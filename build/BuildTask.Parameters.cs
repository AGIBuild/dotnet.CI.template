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

    [Parameter("Host to publish - supported values are 'web' and 'cli'")]
    readonly string PublishHost = string.Empty;

    // Internal path conventions controlled by build tasks (not external parameters).
    readonly string BuildPath = "ChengYuan.slnx";
    readonly string TestPath = "ChengYuan.slnx";
    readonly string PackPath = "ChengYuan.slnx";
    string NormalizedHost => PublishHost.Trim().ToLowerInvariant();
    string PublishPath => NormalizedHost switch
    {
        "web" => "src/Hosts/WebHost/ChengYuan.WebHost.csproj",
        "cli" => "src/Hosts/CliHost/ChengYuan.CliHost.csproj",
        _ => string.Empty
    };

    // Publish inputs.
    [Parameter("Target runtime identifier used by Publish target")]
    readonly string Runtime = string.Empty;

    [Parameter("Publish self-contained output in Publish target")]
    readonly bool SelfContained;

    [Parameter("Port used by Doc target")]
    readonly int DocsPort = 5173;

    [Parameter("NuGet API key for PushNuGetPackages target. Defaults to NUGET_API_KEY environment variable.")]
    readonly string NuGetApiKey = string.Empty;

    [Parameter("Minimum line coverage percentage (0-100). CoverageReport fails if below this threshold.")]
    readonly int CoverageThreshold = 90;

    [Parameter("Minimum branch coverage percentage (0-100). CoverageReport fails if below this threshold.")]
    readonly int BranchCoverageThreshold = 90;

    [Parameter("New project name for Init target (e.g. Acme.Payments)")]
    readonly string ProjectName = string.Empty;

    [Parameter("Author / organization for Init target")]
    readonly string Author = string.Empty;

    [Parameter("Reset git history to a fresh commit during Init")]
    readonly bool ResetGit;

    // Common output/input locations.
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    AbsolutePath TestResultsDirectory => ArtifactsDirectory / "test-results";
    AbsolutePath PackagesDirectory => ArtifactsDirectory / "packages";
    AbsolutePath PublishDirectory => ArtifactsDirectory / "publish";
    AbsolutePath InstallersDirectory => ArtifactsDirectory / "installers";
    AbsolutePath DocsDirectory => RootDirectory / "docs";
    AbsolutePath DocsNodeModulesDirectory => DocsDirectory / "node_modules";
    AbsolutePath DocsPackageLockFile => DocsDirectory / "package-lock.json";

    AbsolutePath ReleaseManifestFile => PackagesDirectory / "release-manifest.json";
    AbsolutePath BuildOutputsMarkerFile => ArtifactsDirectory / ".build-outputs" / "build-outputs.json";
    AbsolutePath ToolManifestFile => RootDirectory / ".config" / "dotnet-tools.json";
    AbsolutePath DirectoryBuildPropsFile => RootDirectory / "Directory.Build.props";
}
