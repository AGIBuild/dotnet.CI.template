using System;
using Nuke.Common;
using Nuke.Common.IO;

partial class BuildTask
{
    Target PackageApp => _ => _
        .DependsOn(Publish)
        .Requires(() => !string.IsNullOrWhiteSpace(Runtime))
        .Requires(() => !string.IsNullOrWhiteSpace(PublishPath))
        .Executes(() =>
        {
            InstallersDirectory.CreateOrCleanDirectory();
            var sourceDir = PublishDirectory / NormalizedHost / Runtime;
            var zipFile = InstallersDirectory / $"app-{NormalizedHost}-{Runtime}.zip";
            sourceDir.ZipTo(zipFile);
            Console.WriteLine($"Created installer: {zipFile}");
        });
}
