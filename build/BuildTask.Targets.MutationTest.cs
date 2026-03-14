using Nuke.Common;
using Nuke.Common.IO;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class BuildTask
{
    AbsolutePath MutationResultsDirectory => ArtifactsDirectory / "mutation-results";

    Target MutationTest => _ => _
        .DependsOn(Restore)
        .Description("Run Stryker.NET mutation testing. Install via: dotnet tool install dotnet-stryker")
        .Executes(() =>
        {
            MutationResultsDirectory.CreateOrCleanDirectory();

            var testProjects = RootDirectory.GlobFiles("tests/**/*.csproj");
            if (testProjects.Count == 0)
            {
                Serilog.Log.Warning("No test projects found under tests/. Skipping.");
                return;
            }

            foreach (var testProject in testProjects)
            {
                DotNet(
                    $"stryker " +
                    $"--project {testProject} " +
                    $"--output {MutationResultsDirectory} " +
                    $"--reporter html --reporter progress",
                    workingDirectory: testProject.Parent);
            }
        });
}
