using System;
using Nuke.Common;

partial class BuildTask
{
    Target Init => _ => _
        .Executes(() =>
        {
            throw new InvalidOperationException(
                "The legacy initialization flow is disabled while the ChengYuan template family implementation is in progress. " +
                "Rename the repository explicitly and extend the framework and application modules directly.");
        });
}
