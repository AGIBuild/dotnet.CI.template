using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class BuildTask
{
    Target Publish => _ => _
        .DependsOn(Restore)
        .Requires(() => !string.IsNullOrWhiteSpace(Runtime))
        .Executes(() =>
        {
            var outputDirectory = PublishDirectory / Runtime;

            DotNetPublish(settings =>
            {
                settings = settings
                    .SetProject(PublishPath)
                    .SetConfiguration(Configuration)
                    .SetRuntime(Runtime)
                    .SetSelfContained(SelfContained)
                    .SetOutput(outputDirectory)
                    .SetNoLogo(true);

                if (!string.IsNullOrWhiteSpace(VersionSuffix))
                    settings = settings.SetVersionSuffix(VersionSuffix);

                return settings;
            });
        });
}
