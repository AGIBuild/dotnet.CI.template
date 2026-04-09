using Nuke.Common;

partial class BuildTask
{
    Target Doc => _ => _
        .DependsOn(DocsBuild)
        .Executes(() =>
        {
            Serilog.Log.Information("Starting docs development server on http://localhost:{DocsPort}.", DocsPort);
            RunNpmCommandAndAssert($"run docs:dev:restart -- --port {DocsPort}", DocsDirectory);
        });

    Target DocsBuild => _ => _
        .Executes(() =>
        {
            EnsureDocsDependencies();
            BuildDocsSite();
        });
}
