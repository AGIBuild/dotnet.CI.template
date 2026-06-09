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
            DotNet($"test --project {RootDirectory / "tests" / "ChengYuan.FrameworkKernel.Tests" / "ChengYuan.FrameworkKernel.Tests.csproj"} --configuration {Configuration} --no-build --results-directory {TestResultsDirectory} -- --report-xunit-trx --report-xunit-trx-filename framework-kernel.trx");
        });
}
