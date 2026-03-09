using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class BuildTask
{
    Target Build => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(settings =>
            {
                settings = settings
                    .SetProjectFile(BuildPath)
                    .SetConfiguration(Configuration)
                    .EnableNoRestore()
                    .SetNoLogo(true);

                if (!string.IsNullOrWhiteSpace(VersionSuffix))
                    settings = settings.SetVersionSuffix(VersionSuffix);

                return settings;
            });

            WriteBuildOutputsMarker();
        });
}
