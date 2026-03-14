using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class BuildTask
{
    Target Pack => _ => _
        .DependsOn(Restore, Build, Test)
        .Executes(() =>
        {
            PackagesDirectory.CreateOrCleanDirectory();
            DotNetPack(settings =>
            {
                settings = settings
                    .SetProject(PackPath)
                    .SetConfiguration(Configuration)
                    .SetSymbolPackageFormat(DotNetSymbolPackageFormat.snupkg)
                    .EnableIncludeSymbols()
                    .EnableIncludeSource()
                    .SetOutputDirectory(PackagesDirectory)
                    .SetNoLogo(true);

                if (CanSkipBuild(PackPath))
                    settings = settings.EnableNoBuild();

                return ApplyVersionSuffix(settings);
            });
        });
}
