using System;
using System.Diagnostics;
using System.IO;
using Nuke.Common;
using Nuke.Common.Tools.DotNet;

partial class BuildTask
{
    DotNetBuildSettings ApplyVersionSuffix(DotNetBuildSettings settings) =>
        string.IsNullOrWhiteSpace(VersionSuffix) ? settings : settings.SetVersionSuffix(VersionSuffix);

    DotNetPackSettings ApplyVersionSuffix(DotNetPackSettings settings) =>
        string.IsNullOrWhiteSpace(VersionSuffix) ? settings : settings.SetVersionSuffix(VersionSuffix);

    DotNetPublishSettings ApplyVersionSuffix(DotNetPublishSettings settings) =>
        string.IsNullOrWhiteSpace(VersionSuffix) ? settings : settings.SetVersionSuffix(VersionSuffix);

    DotNetTestSettings ApplyVersionSuffix(DotNetTestSettings settings) =>
        string.IsNullOrWhiteSpace(VersionSuffix) ? settings : settings.SetProperty("VersionSuffix", VersionSuffix);

    void EnsureDocsDependencies()
    {
        if (Directory.Exists(DocsNodeModulesDirectory))
        {
            Serilog.Log.Information("Skipping docs dependency install because node_modules already exists.");
            return;
        }

        var installCommand = File.Exists(DocsPackageLockFile) ? "ci" : "install";
        Serilog.Log.Information("Installing docs dependencies with 'npm {InstallCommand}'.", installCommand);
        RunNpmCommandAndAssert(installCommand, DocsDirectory);
    }

    void BuildDocsSite()
    {
        Serilog.Log.Information("Building docs site.");
        RunNpmCommandAndAssert("run docs:build", DocsDirectory);
    }

    void RunNpmCommandAndAssert(string arguments, string workingDirectory)
    {
        var (fileName, resolvedArguments) = CreateNpmInvocation(arguments);
        RunCommandAndAssert(fileName, resolvedArguments, workingDirectory);
    }

    (string FileName, string Arguments) CreateNpmInvocation(string arguments)
    {
        return OperatingSystem.IsWindows()
            ? ("cmd.exe", $"/c npm.cmd {arguments}")
            : ("npm", arguments);
    }

    void RunCommandAndAssert(string fileName, string arguments, string workingDirectory)
    {
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            CreateNoWindow = true
        }) ?? throw new InvalidOperationException($"Failed to start process '{fileName}'.");

        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Process '{fileName} {arguments}' exited with code {process.ExitCode}.");
        }
    }

}
