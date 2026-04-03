using Nuke.Common;
using Nuke.Common.IO;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class BuildTask
{
    Target Test => _ => _
        .DependsOn(Restore, Build)
        .Executes(() =>
        {
            TestResultsDirectory.CreateOrCleanDirectory();
            DotNet($"test --solution {TestPath} --configuration {Configuration} --no-build --nologo");
        });
}
