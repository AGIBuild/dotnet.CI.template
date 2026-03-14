using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class BuildTask
{
    Target Format => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNet($"format {BuildPath} --verify-no-changes", workingDirectory: RootDirectory);
        });
}
