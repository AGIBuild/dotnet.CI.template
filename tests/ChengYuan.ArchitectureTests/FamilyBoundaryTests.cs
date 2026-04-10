using Shouldly;

namespace ChengYuan.ArchitectureTests;

public class FamilyBoundaryTests
{
    [Fact]
    public void FrameworkProjects_ShouldNotReferenceApplicationProjects()
    {
        var repositoryRoot = FindRepositoryRoot();
        var frameworkProjects = Directory.EnumerateFiles(Path.Combine(repositoryRoot, "src", "Framework"), "*.csproj", SearchOption.AllDirectories);

        foreach (var projectFile in frameworkProjects)
        {
            var projectXml = File.ReadAllText(projectFile);
            projectXml.ShouldNotContain("src\\Applications\\");
            projectXml.ShouldNotContain("src/Applications/");
        }
    }

    [Fact]
    public void NonHostingProjects_ShouldNotReferenceHostingProject()
    {
        var repositoryRoot = FindRepositoryRoot();
        var sourceProjects = Directory.EnumerateFiles(Path.Combine(repositoryRoot, "src"), "*.csproj", SearchOption.AllDirectories)
            .Where(projectFile => !projectFile.EndsWith("ChengYuan.Hosting.csproj", StringComparison.OrdinalIgnoreCase));

        foreach (var projectFile in sourceProjects)
        {
            var projectXml = File.ReadAllText(projectFile);
            projectXml.ShouldNotContain("ChengYuan.Hosting\\ChengYuan.Hosting.csproj");
            projectXml.ShouldNotContain("ChengYuan.Hosting/ChengYuan.Hosting.csproj");
        }
    }

    [Fact]
    public void CoreProject_ShouldNotReferenceDomainProjects()
    {
        var repositoryRoot = FindRepositoryRoot();
        var projectXml = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Framework", "ChengYuan.Core", "ChengYuan.Core.csproj"));

        projectXml.ShouldNotContain("ChengYuan.Domain");
        projectXml.ShouldNotContain("<ProjectReference");
    }

    [Fact]
    public void SettingManagementPersistenceProject_ShouldReferenceApplicationAndCoreOnly()
    {
        var repositoryRoot = FindRepositoryRoot();
        var projectXml = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Applications", "ChengYuan.SettingManagement", "ChengYuan.SettingManagement.Persistence", "ChengYuan.SettingManagement.Persistence.csproj"));

        projectXml.ShouldContain("ChengYuan.SettingManagement.Application");
        projectXml.ShouldContain("ChengYuan.Core\\ChengYuan.Core.csproj");
        projectXml.ShouldNotContain("ChengYuan.Hosting");
        projectXml.ShouldNotContain("ChengYuan.PermissionManagement");
        projectXml.ShouldNotContain("ChengYuan.FeatureManagement");
        projectXml.ShouldNotContain("ChengYuan.AuditLogging");
        projectXml.ShouldNotContain("ChengYuan.TenantManagement");
    }

    [Fact]
    public void PermissionManagementPersistenceProject_ShouldReferenceApplicationAndCoreOnly()
    {
        var repositoryRoot = FindRepositoryRoot();
        var projectXml = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Applications", "ChengYuan.PermissionManagement", "ChengYuan.PermissionManagement.Persistence", "ChengYuan.PermissionManagement.Persistence.csproj"));

        projectXml.ShouldContain("ChengYuan.PermissionManagement.Application");
        projectXml.ShouldContain("ChengYuan.Core\\ChengYuan.Core.csproj");
        projectXml.ShouldNotContain("ChengYuan.Hosting");
        projectXml.ShouldNotContain("ChengYuan.SettingManagement");
        projectXml.ShouldNotContain("ChengYuan.FeatureManagement");
        projectXml.ShouldNotContain("ChengYuan.AuditLogging");
        projectXml.ShouldNotContain("ChengYuan.TenantManagement");
    }

    [Fact]
    public void FeatureManagementPersistenceProject_ShouldReferenceApplicationAndCoreOnly()
    {
        var repositoryRoot = FindRepositoryRoot();
        var projectXml = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Applications", "ChengYuan.FeatureManagement", "ChengYuan.FeatureManagement.Persistence", "ChengYuan.FeatureManagement.Persistence.csproj"));

        projectXml.ShouldContain("ChengYuan.FeatureManagement.Application");
        projectXml.ShouldContain("ChengYuan.Core\\ChengYuan.Core.csproj");
        projectXml.ShouldNotContain("ChengYuan.Hosting");
        projectXml.ShouldNotContain("ChengYuan.SettingManagement");
        projectXml.ShouldNotContain("ChengYuan.PermissionManagement");
        projectXml.ShouldNotContain("ChengYuan.AuditLogging");
        projectXml.ShouldNotContain("ChengYuan.TenantManagement");
    }

    [Fact]
    public void AuditLoggingPersistenceProject_ShouldReferenceApplicationAndCoreOnly()
    {
        var repositoryRoot = FindRepositoryRoot();
        var projectXml = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Applications", "ChengYuan.AuditLogging", "ChengYuan.AuditLogging.Persistence", "ChengYuan.AuditLogging.Persistence.csproj"));

        projectXml.ShouldContain("ChengYuan.AuditLogging.Application");
        projectXml.ShouldContain("ChengYuan.Core\\ChengYuan.Core.csproj");
        projectXml.ShouldNotContain("ChengYuan.Hosting");
        projectXml.ShouldNotContain("ChengYuan.SettingManagement");
        projectXml.ShouldNotContain("ChengYuan.PermissionManagement");
        projectXml.ShouldNotContain("ChengYuan.FeatureManagement");
        projectXml.ShouldNotContain("ChengYuan.TenantManagement");
    }

    [Fact]
    public void IdentityContractsProject_ShouldNotReferenceApplicationOrPersistenceProjects()
    {
        var repositoryRoot = FindRepositoryRoot();
        var projectXml = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Applications", "ChengYuan.Identity", "ChengYuan.Identity.Contracts", "ChengYuan.Identity.Contracts.csproj"));

        projectXml.ShouldNotContain("ChengYuan.Identity.Application");
        projectXml.ShouldNotContain("ChengYuan.Identity.Domain");
        projectXml.ShouldNotContain("ChengYuan.Identity.Persistence");
        projectXml.ShouldNotContain("ChengYuan.Hosting");
    }

    [Fact]
    public void IdentityDomainProject_ShouldReferenceContractsAndCoreOnly()
    {
        var repositoryRoot = FindRepositoryRoot();
        var projectXml = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Applications", "ChengYuan.Identity", "ChengYuan.Identity.Domain", "ChengYuan.Identity.Domain.csproj"));

        projectXml.ShouldContain("ChengYuan.Identity.Contracts");
        projectXml.ShouldContain("ChengYuan.Core\\ChengYuan.Core.csproj");
        projectXml.ShouldNotContain("ChengYuan.Identity.Application");
        projectXml.ShouldNotContain("ChengYuan.Identity.Persistence");
        projectXml.ShouldNotContain("ChengYuan.Hosting");
    }

    [Fact]
    public void IdentityApplicationProject_ShouldReferenceContractsDomainAndCoreOnly()
    {
        var repositoryRoot = FindRepositoryRoot();
        var projectXml = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Applications", "ChengYuan.Identity", "ChengYuan.Identity.Application", "ChengYuan.Identity.Application.csproj"));

        projectXml.ShouldContain("ChengYuan.Identity.Contracts");
        projectXml.ShouldContain("ChengYuan.Identity.Domain");
        projectXml.ShouldContain("ChengYuan.Core\\ChengYuan.Core.csproj");
        projectXml.ShouldNotContain("ChengYuan.Identity.Persistence");
        projectXml.ShouldNotContain("ChengYuan.Hosting");
    }

    [Fact]
    public void IdentityPersistenceProject_ShouldReferenceApplicationDomainAndCoreOnly()
    {
        var repositoryRoot = FindRepositoryRoot();
        var projectXml = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Applications", "ChengYuan.Identity", "ChengYuan.Identity.Persistence", "ChengYuan.Identity.Persistence.csproj"));

        projectXml.ShouldContain("ChengYuan.Identity.Application");
        projectXml.ShouldContain("ChengYuan.Identity.Domain");
        projectXml.ShouldContain("ChengYuan.Core\\ChengYuan.Core.csproj");
        projectXml.ShouldNotContain("ChengYuan.Hosting");
        projectXml.ShouldNotContain("ChengYuan.SettingManagement");
        projectXml.ShouldNotContain("ChengYuan.PermissionManagement");
        projectXml.ShouldNotContain("ChengYuan.FeatureManagement");
        projectXml.ShouldNotContain("ChengYuan.TenantManagement");
    }

    [Fact]
    public void IdentityWebProject_ShouldReferenceApplicationAndCoreOnly()
    {
        var repositoryRoot = FindRepositoryRoot();
        var projectXml = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Applications", "ChengYuan.Identity", "ChengYuan.Identity.Web", "ChengYuan.Identity.Web.csproj"));

        projectXml.ShouldContain("ChengYuan.Identity.Application");
        projectXml.ShouldContain("ChengYuan.Core\\ChengYuan.Core.csproj");
        projectXml.ShouldContain("Microsoft.AspNetCore.App");
        projectXml.ShouldNotContain("ChengYuan.Identity.Persistence");
        projectXml.ShouldNotContain("ChengYuan.Hosting");
    }

    private static string FindRepositoryRoot()
    {
        var currentDirectory = new DirectoryInfo(AppContext.BaseDirectory);

        while (currentDirectory is not null && !IsRepositoryRoot(currentDirectory.FullName))
            currentDirectory = currentDirectory.Parent;

        return currentDirectory?.FullName
            ?? throw new InvalidOperationException("Unable to find the repository root.");
    }

    private static bool IsRepositoryRoot(string directory)
    {
        return File.Exists(Path.Combine(directory, "Directory.Build.props"))
            && File.Exists(Path.Combine(directory, "Directory.Packages.props"))
            && File.Exists(Path.Combine(directory, "ChengYuan.slnx"));
    }
}
