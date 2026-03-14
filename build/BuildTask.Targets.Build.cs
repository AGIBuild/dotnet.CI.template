using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class BuildTask
{
    Target Build => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(settings => ApplyVersionSuffix(settings
                    .SetProjectFile(BuildPath)
                    .SetConfiguration(Configuration)
                    .EnableNoRestore()
                    .SetNoLogo(true)));

            WriteBuildOutputsMarker();
        });
}
