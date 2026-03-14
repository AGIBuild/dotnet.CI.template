using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Nuke.Common;
using Nuke.Common.IO;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class BuildTask
{
    const string OldSampleName = "Dotnet.CI.Template.Sample";
    const string OldSlnName = "Dotnet.CI.Template";

    static readonly string[] ExcludedDirectories =
        [".git", "node_modules", "artifacts", "dist", "bin", "obj", ".vitepress"];

    Target Init => _ => _
        .Requires(() => !string.IsNullOrWhiteSpace(ProjectName))
        .Requires(() => Regex.IsMatch(ProjectName, @"^[A-Za-z][A-Za-z0-9.]*$"))
        .Executes(() =>
        {
            Console.WriteLine($"[1/7] Replacing file contents...");
            ReplaceFileContents();

            Console.WriteLine($"[2/7] Updating author...");
            UpdateAuthor();

            Console.WriteLine($"[3/7] Renaming directories...");
            RenameDirectories();

            Console.WriteLine($"[4/7] Renaming files...");
            RenameFiles();

            Console.WriteLine($"[5/7] Resetting version to 0.1.0...");
            ResetVersion();

            Console.WriteLine($"[6/7] Refreshing lock files...");
            DotNet($"restore {ProjectName}.slnx --force-evaluate", workingDirectory: RootDirectory);

            if (ResetGit)
            {
                Console.WriteLine("[7/7] Resetting git history...");
                ResetGitHistory();
            }
            else
            {
                Console.WriteLine("[7/7] Git history preserved.");
            }

            DeleteInitScripts();

            Console.WriteLine();
            Console.WriteLine($"Done! Your project '{ProjectName}' is ready.");
            Console.WriteLine();
            Console.WriteLine("Next steps:");
            Console.WriteLine("  1. Update GitHub URL in docs/.vitepress/config.ts");
            Console.WriteLine("  2. Run: dotnet build");
        });

    void ReplaceFileContents()
    {
        var files = RootDirectory
            .GlobFiles("**/*")
            .Where(f => !IsExcludedPath(f) && !IsInitScript(f) && IsTextFile(f));

        foreach (var file in files)
        {
            var content = File.ReadAllText(file);
            if (!content.Contains(OldSampleName) && !content.Contains(OldSlnName))
                continue;

            content = content.Replace(OldSampleName, ProjectName);
            content = content.Replace(OldSlnName, ProjectName);
            File.WriteAllText(file, content);
        }
    }

    void UpdateAuthor()
    {
        if (string.IsNullOrWhiteSpace(Author))
            return;

        var csprojFiles = RootDirectory
            .GlobFiles("src/**/*.csproj")
            .Where(f => !IsExcludedPath(f));

        foreach (var csproj in csprojFiles)
        {
            var content = File.ReadAllText(csproj);
            if (!content.Contains("<Authors>"))
                continue;

            content = Regex.Replace(content, @"<Authors>[^<]*</Authors>", $"<Authors>{Author}</Authors>");
            File.WriteAllText(csproj, content);
        }
    }

    void RenameDirectories()
    {
        var dirs = Directory.GetDirectories(RootDirectory, $"*{OldSampleName}*", SearchOption.AllDirectories)
            .Where(d => !IsExcludedPath((AbsolutePath)d))
            .OrderByDescending(d => d.Length);

        foreach (var dir in dirs)
        {
            var newName = Path.GetFileName(dir).Replace(OldSampleName, ProjectName);
            var newPath = Path.Combine(Path.GetDirectoryName(dir)!, newName);
            Console.WriteLine($"  {dir} -> {newPath}");
            Directory.Move(dir, newPath);
        }
    }

    void RenameFiles()
    {
        var files = Directory.GetFiles(RootDirectory, $"*{OldSampleName}*", SearchOption.AllDirectories)
            .Where(f => !IsExcludedPath((AbsolutePath)f));

        foreach (var file in files)
        {
            var newName = Path.GetFileName(file).Replace(OldSampleName, ProjectName);
            var newPath = Path.Combine(Path.GetDirectoryName(file)!, newName);
            Console.WriteLine($"  {file} -> {newPath}");
            File.Move(file, newPath);
        }

        var slnFile = RootDirectory / $"{OldSlnName}.slnx";
        if (File.Exists(slnFile))
        {
            var newSlnFile = RootDirectory / $"{ProjectName}.slnx";
            File.Move(slnFile, newSlnFile);
            Console.WriteLine($"  {slnFile} -> {newSlnFile}");
        }
    }

    void ResetVersion()
    {
        var propsFile = DirectoryBuildPropsFile;
        if (!File.Exists(propsFile))
            return;

        var content = File.ReadAllText(propsFile);
        content = Regex.Replace(content, @"<VersionPrefix>[^<]*</VersionPrefix>", "<VersionPrefix>0.1.0</VersionPrefix>");
        File.WriteAllText(propsFile, content);
    }

    void ResetGitHistory()
    {
        var gitDir = RootDirectory / ".git";
        if (Directory.Exists(gitDir))
            Directory.Delete(gitDir, recursive: true);

        RunGit("init");
        RunGit("add .");
        RunGit("commit -m \"Initial commit from dotnet.CI.template\"");
    }

    void DeleteInitScripts()
    {
        File.Delete(RootDirectory / "init.sh");
        File.Delete(RootDirectory / "init.ps1");
    }

    void RunGit(string arguments)
    {
        var psi = new ProcessStartInfo("git", arguments)
        {
            WorkingDirectory = RootDirectory,
            UseShellExecute = false
        };
        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException($"Failed to start: git {arguments}");
        process.WaitForExit();
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"git {arguments} exited with code {process.ExitCode}");
    }

    bool IsExcludedPath(AbsolutePath path)
    {
        var relative = RootDirectory.GetRelativePathTo(path).ToString();
        return ExcludedDirectories.Any(d =>
            relative.Contains($"{d}/", StringComparison.OrdinalIgnoreCase) ||
            relative.Contains($"{d}\\", StringComparison.OrdinalIgnoreCase) ||
            relative.StartsWith($"{d}/", StringComparison.OrdinalIgnoreCase) ||
            relative.StartsWith($"{d}\\", StringComparison.OrdinalIgnoreCase));
    }

    static bool IsInitScript(AbsolutePath path)
    {
        var name = Path.GetFileName(path);
        return name is "init.sh" or "init.ps1";
    }

    static bool IsTextFile(AbsolutePath path)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();
        return extension is ".cs" or ".csproj" or ".props" or ".targets" or ".slnx" or ".sln"
            or ".json" or ".yml" or ".yaml" or ".xml" or ".md" or ".txt" or ".sh" or ".ps1"
            or ".ts" or ".js" or ".css" or ".html" or ".editorconfig" or ".gitignore"
            or ".config" or ".mdc" or ".toml";
    }
}
