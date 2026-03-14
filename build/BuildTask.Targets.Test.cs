using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class BuildTask
{
    Target Test => _ => _
        .DependsOn(Restore, Build)
        .Executes(() =>
        {
            TestResultsDirectory.CreateOrCleanDirectory();
            var useNoBuild = CanSkipBuild(TestPath);
            DotNetTest(settings =>
            {
                settings = settings
                    .SetProjectFile(TestPath)
                    .SetConfiguration(Configuration)
                    .SetLoggers("trx;LogFileName=test-results.trx")
                    .SetResultsDirectory(TestResultsDirectory)
                    .SetDataCollector("XPlat Code Coverage")
                    .SetNoLogo(true);

                if (useNoBuild)
                    settings = settings.EnableNoBuild();
                else
                    settings = ApplyVersionSuffix(settings);

                return settings;
            });

            if (!useNoBuild)
                WriteBuildOutputsMarker();
        });
}
