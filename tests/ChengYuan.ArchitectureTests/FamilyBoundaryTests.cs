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
    public void CoreProject_ShouldNotReferenceProviderOrDomainProjects()
    {
        var repositoryRoot = FindRepositoryRoot();
        var projectXml = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Framework", "ChengYuan.Core", "ChengYuan.Core", "ChengYuan.Core.csproj"));

        projectXml.ShouldNotContain("ChengYuan.Core.Json");
        projectXml.ShouldNotContain("ChengYuan.Core.EntityFrameworkCore");
        projectXml.ShouldNotContain("ChengYuan.Domain");
    }

    [Fact]
    public void CoreJsonProject_ShouldNotReferenceEntityFrameworkCoreOrDomainProjects()
    {
        var repositoryRoot = FindRepositoryRoot();
        var projectXml = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Framework", "ChengYuan.Core", "ChengYuan.Core.Json", "ChengYuan.Core.Json.csproj"));

        projectXml.ShouldNotContain("ChengYuan.Core.EntityFrameworkCore");
        projectXml.ShouldNotContain("ChengYuan.Domain");
    }

    [Fact]
    public void CoreValidationProject_ShouldNotReferenceLocalizationOrProviderProjects()
    {
        var repositoryRoot = FindRepositoryRoot();
        var projectXml = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Framework", "ChengYuan.Core", "ChengYuan.Core.Validation", "ChengYuan.Core.Validation.csproj"));

        projectXml.ShouldNotContain("ChengYuan.Core.Localization");
        projectXml.ShouldNotContain("ChengYuan.Core.Data");
        projectXml.ShouldNotContain("ChengYuan.Core.EntityFrameworkCore");
        projectXml.ShouldNotContain("ChengYuan.Domain");
    }

    [Fact]
    public void CoreLocalizationProject_ShouldNotReferenceValidationOrProviderProjects()
    {
        var repositoryRoot = FindRepositoryRoot();
        var projectXml = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Framework", "ChengYuan.Core", "ChengYuan.Core.Localization", "ChengYuan.Core.Localization.csproj"));

        projectXml.ShouldNotContain("ChengYuan.Core.Validation");
        projectXml.ShouldNotContain("ChengYuan.Core.Data");
        projectXml.ShouldNotContain("ChengYuan.Core.EntityFrameworkCore");
        projectXml.ShouldNotContain("ChengYuan.Domain");
    }

    [Fact]
    public void CoreDataProject_ShouldNotReferenceEntityFrameworkCoreOrDomainProjects()
    {
        var repositoryRoot = FindRepositoryRoot();
        var projectXml = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Framework", "ChengYuan.Core", "ChengYuan.Core.Data", "ChengYuan.Core.Data.csproj"));

        projectXml.ShouldNotContain("ChengYuan.Core.EntityFrameworkCore");
        projectXml.ShouldNotContain("ChengYuan.Domain");
    }

    [Fact]
    public void CoreEntityFrameworkCoreProject_ShouldReferenceCoreDataAndNotReferenceDomainProjects()
    {
        var repositoryRoot = FindRepositoryRoot();
        var projectXml = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Framework", "ChengYuan.Core", "ChengYuan.Core.EntityFrameworkCore", "ChengYuan.Core.EntityFrameworkCore.csproj"));

        projectXml.ShouldContain("ChengYuan.Core.Data");
        projectXml.ShouldNotContain("ChengYuan.Domain");
    }

    [Fact]
    public void SettingManagementPersistenceProject_ShouldReferenceApplicationAndApprovedCoreDataProjectsOnly()
    {
        var repositoryRoot = FindRepositoryRoot();
        var projectXml = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Applications", "ChengYuan.SettingManagement", "ChengYuan.SettingManagement.Persistence", "ChengYuan.SettingManagement.Persistence.csproj"));

        projectXml.ShouldContain("ChengYuan.SettingManagement.Application");
        projectXml.ShouldContain("ChengYuan.Core.Runtime");
        projectXml.ShouldContain("ChengYuan.Core.Data");
        projectXml.ShouldContain("ChengYuan.Core.EntityFrameworkCore");
        projectXml.ShouldNotContain("ChengYuan.Hosting");
        projectXml.ShouldNotContain("ChengYuan.PermissionManagement");
        projectXml.ShouldNotContain("ChengYuan.FeatureManagement");
        projectXml.ShouldNotContain("ChengYuan.AuditLogging");
        projectXml.ShouldNotContain("ChengYuan.TenantManagement");
    }

    [Fact]
    public void PermissionManagementPersistenceProject_ShouldReferenceApplicationAndApprovedCoreDataProjectsOnly()
    {
        var repositoryRoot = FindRepositoryRoot();
        var projectXml = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Applications", "ChengYuan.PermissionManagement", "ChengYuan.PermissionManagement.Persistence", "ChengYuan.PermissionManagement.Persistence.csproj"));

        projectXml.ShouldContain("ChengYuan.PermissionManagement.Application");
        projectXml.ShouldContain("ChengYuan.Core.Runtime");
        projectXml.ShouldContain("ChengYuan.Core.Data");
        projectXml.ShouldContain("ChengYuan.Core.EntityFrameworkCore");
        projectXml.ShouldNotContain("ChengYuan.Hosting");
        projectXml.ShouldNotContain("ChengYuan.SettingManagement");
        projectXml.ShouldNotContain("ChengYuan.FeatureManagement");
        projectXml.ShouldNotContain("ChengYuan.AuditLogging");
        projectXml.ShouldNotContain("ChengYuan.TenantManagement");
    }

    [Fact]
    public void FeatureManagementPersistenceProject_ShouldReferenceApplicationAndApprovedCoreDataProjectsOnly()
    {
        var repositoryRoot = FindRepositoryRoot();
        var projectXml = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Applications", "ChengYuan.FeatureManagement", "ChengYuan.FeatureManagement.Persistence", "ChengYuan.FeatureManagement.Persistence.csproj"));

        projectXml.ShouldContain("ChengYuan.FeatureManagement.Application");
        projectXml.ShouldContain("ChengYuan.Core.Runtime");
        projectXml.ShouldContain("ChengYuan.Core.Data");
        projectXml.ShouldContain("ChengYuan.Core.EntityFrameworkCore");
        projectXml.ShouldNotContain("ChengYuan.Hosting");
        projectXml.ShouldNotContain("ChengYuan.SettingManagement");
        projectXml.ShouldNotContain("ChengYuan.PermissionManagement");
        projectXml.ShouldNotContain("ChengYuan.AuditLogging");
        projectXml.ShouldNotContain("ChengYuan.TenantManagement");
    }

    [Fact]
    public void AuditLoggingPersistenceProject_ShouldReferenceApplicationAndApprovedCoreDataProjectsOnly()
    {
        var repositoryRoot = FindRepositoryRoot();
        var projectXml = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Applications", "ChengYuan.AuditLogging", "ChengYuan.AuditLogging.Persistence", "ChengYuan.AuditLogging.Persistence.csproj"));

        projectXml.ShouldContain("ChengYuan.AuditLogging.Application");
        projectXml.ShouldContain("ChengYuan.Core.Runtime");
        projectXml.ShouldContain("ChengYuan.Core.Data");
        projectXml.ShouldContain("ChengYuan.Core.EntityFrameworkCore");
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
    public void IdentityDomainProject_ShouldReferenceContractsAndApprovedCoreProjectsOnly()
    {
        var repositoryRoot = FindRepositoryRoot();
        var projectXml = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Applications", "ChengYuan.Identity", "ChengYuan.Identity.Domain", "ChengYuan.Identity.Domain.csproj"));

        projectXml.ShouldContain("ChengYuan.Identity.Contracts");
        projectXml.ShouldContain("ChengYuan.Core\\ChengYuan.Core\\ChengYuan.Core.csproj");
        projectXml.ShouldContain("ChengYuan.Core.Data");
        projectXml.ShouldNotContain("ChengYuan.Identity.Application");
        projectXml.ShouldNotContain("ChengYuan.Identity.Persistence");
        projectXml.ShouldNotContain("ChengYuan.Hosting");
    }

    [Fact]
    public void IdentityApplicationProject_ShouldReferenceContractsDomainAndApprovedCoreProjectsOnly()
    {
        var repositoryRoot = FindRepositoryRoot();
        var projectXml = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Applications", "ChengYuan.Identity", "ChengYuan.Identity.Application", "ChengYuan.Identity.Application.csproj"));

        projectXml.ShouldContain("ChengYuan.Identity.Contracts");
        projectXml.ShouldContain("ChengYuan.Identity.Domain");
        projectXml.ShouldContain("ChengYuan.Core.Runtime");
        projectXml.ShouldContain("ChengYuan.Core.Data");
        projectXml.ShouldNotContain("ChengYuan.Identity.Persistence");
        projectXml.ShouldNotContain("ChengYuan.Hosting");
    }

    [Fact]
    public void IdentityPersistenceProject_ShouldReferenceDomainAndApprovedCoreDataProjectsOnly()
    {
        var repositoryRoot = FindRepositoryRoot();
        var projectXml = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Applications", "ChengYuan.Identity", "ChengYuan.Identity.Persistence", "ChengYuan.Identity.Persistence.csproj"));

        projectXml.ShouldContain("ChengYuan.Identity.Application");
        projectXml.ShouldContain("ChengYuan.Identity.Domain");
        projectXml.ShouldContain("ChengYuan.Core.Runtime");
        projectXml.ShouldContain("ChengYuan.Core.Data");
        projectXml.ShouldContain("ChengYuan.Core.EntityFrameworkCore");
        projectXml.ShouldNotContain("ChengYuan.Hosting");
        projectXml.ShouldNotContain("ChengYuan.SettingManagement");
        projectXml.ShouldNotContain("ChengYuan.PermissionManagement");
        projectXml.ShouldNotContain("ChengYuan.FeatureManagement");
        projectXml.ShouldNotContain("ChengYuan.TenantManagement");
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
