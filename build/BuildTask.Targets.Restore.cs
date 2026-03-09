using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class BuildTask
{
    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            var excluded = new[]
            {
                BuildProjectDirectory / "bin",
                BuildProjectDirectory / "obj"
            };

            RootDirectory.GlobDirectories("**/bin", "**/obj")
                .Where(directory => excluded.All(ex => !directory.Equals(ex)))
                .ForEach(directory => directory.DeleteDirectory());
            ArtifactsDirectory.CreateOrCleanDirectory();
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(settings => settings
                .SetProjectFile(BuildPath));

            if (File.Exists(ToolManifestFile))
            {
                DotNetToolRestore(settings => settings
                    .SetToolManifest(ToolManifestFile));
            }
        });
}
