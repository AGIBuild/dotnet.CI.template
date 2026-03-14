using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

// Uses SDK-built-in `dotnet format`. Version is pinned via global.json,
// ensuring local and CI environments run the same formatter.
partial class BuildTask
{
    Target Format => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNet($"format {BuildPath} --verify-no-changes", workingDirectory: RootDirectory);
        });
}
