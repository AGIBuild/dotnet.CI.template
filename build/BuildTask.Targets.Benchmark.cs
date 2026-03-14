using Nuke.Common;
using Nuke.Common.IO;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class BuildTask
{
    AbsolutePath BenchmarkResultsDirectory => ArtifactsDirectory / "benchmark-results";

    [Parameter("Filter expression for BenchmarkDotNet (e.g., '*Calculator*')")]
    readonly string BenchmarkFilter = "*";

    Target Benchmark => _ => _
        .DependsOn(Restore)
        .Description("Run BenchmarkDotNet benchmarks. Requires a project with BenchmarkDotNet references.")
        .Executes(() =>
        {
            BenchmarkResultsDirectory.CreateOrCleanDirectory();

            var benchProjects = RootDirectory.GlobFiles("benchmarks/**/*.csproj");
            if (benchProjects.Count == 0)
            {
                Serilog.Log.Warning("No benchmark projects found under benchmarks/. Skipping.");
                return;
            }

            foreach (var project in benchProjects)
            {
                DotNet(
                    $"run --project {project} -c Release " +
                    $"-- --filter {BenchmarkFilter} " +
                    $"--artifacts {BenchmarkResultsDirectory}");
            }
        });
}
