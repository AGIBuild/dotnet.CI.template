using Nuke.Common.Tools.DotNet;

partial class BuildTask
{
    DotNetBuildSettings ApplyVersionSuffix(DotNetBuildSettings settings) =>
        string.IsNullOrWhiteSpace(VersionSuffix) ? settings : settings.SetVersionSuffix(VersionSuffix);

    DotNetPackSettings ApplyVersionSuffix(DotNetPackSettings settings) =>
        string.IsNullOrWhiteSpace(VersionSuffix) ? settings : settings.SetVersionSuffix(VersionSuffix);

    DotNetPublishSettings ApplyVersionSuffix(DotNetPublishSettings settings) =>
        string.IsNullOrWhiteSpace(VersionSuffix) ? settings : settings.SetVersionSuffix(VersionSuffix);

    DotNetTestSettings ApplyVersionSuffix(DotNetTestSettings settings) =>
        string.IsNullOrWhiteSpace(VersionSuffix) ? settings : settings.SetProperty("VersionSuffix", VersionSuffix);
}
