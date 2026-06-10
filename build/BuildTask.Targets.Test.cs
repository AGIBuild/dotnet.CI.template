using System;
using System.IO;
using System.Linq;
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
            var testProjects = RootDirectory.GlobFiles("tests/**/*.csproj")
                .OrderBy(static project => project.ToString(), StringComparer.Ordinal);

            foreach (var testProject in testProjects)
            {
                var trxFileName = $"{Path.GetFileNameWithoutExtension(testProject.ToString())}.trx";
                DotNet($"test --project {testProject} --configuration {Configuration} --no-build --results-directory {TestResultsDirectory} -- --report-xunit-trx --report-xunit-trx-filename {trxFileName}");
            }
        });
}
