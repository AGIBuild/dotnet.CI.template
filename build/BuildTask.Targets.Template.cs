using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class BuildTask
{
    AbsolutePath TemplateSmokeDirectory => ArtifactsDirectory / "template-smoke";

    Target TemplateSmoke => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            TemplateSmokeDirectory.CreateOrCleanDirectory();

            DotNet($"new install {RootDirectory} --force", workingDirectory: TemplateSmokeDirectory);
            DotNet("new chengyuan-modular -n Smoke.App --output generated", workingDirectory: TemplateSmokeDirectory);
            DotNet($"restore {TemplateSmokeDirectory / "generated" / "Smoke.App.slnx"}", workingDirectory: TemplateSmokeDirectory);
            DotNet($"build {TemplateSmokeDirectory / "generated" / "Smoke.App.slnx"} --configuration {Configuration} --no-restore --nologo", workingDirectory: TemplateSmokeDirectory);
        });
}
