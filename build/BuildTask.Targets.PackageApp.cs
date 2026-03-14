using System;
using Nuke.Common;
using Nuke.Common.IO;

partial class BuildTask
{
    Target PackageApp => _ => _
        .DependsOn(Publish)
        .Requires(() => !string.IsNullOrWhiteSpace(Runtime))
        .Executes(() =>
        {
            InstallersDirectory.CreateOrCleanDirectory();
            var sourceDir = PublishDirectory / Runtime;
            var zipFile = InstallersDirectory / $"app-{Runtime}.zip";
            sourceDir.ZipTo(zipFile);
            Console.WriteLine($"Created installer: {zipFile}");
        });
}
