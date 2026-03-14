using Nuke.Common;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class BuildTask
{
    Target RefreshLockFiles => _ => _
        .Description("Regenerate packages.lock.json files after dependency changes.")
        .Executes(() =>
        {
            DotNet($"restore {BuildPath} --force-evaluate", workingDirectory: RootDirectory);
        });
}
