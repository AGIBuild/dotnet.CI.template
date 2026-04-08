using System.Text.Json;
using ChengYuan.Core.Extensions;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class CoreExtensionModelTests
{
    private static readonly ExtraPropertyManager PropertyManager = CreatePropertyManager();

    [Fact]
    public void SetProperty_ShouldPopulateExtraProperties()
    {
        var workspaceSettings = new WorkspaceSettings();

        workspaceSettings.SetProperty("timezone", "UTC+8");

        workspaceSettings.HasProperty("timezone").ShouldBeTrue();
        workspaceSettings.GetProperty<string>("timezone").ShouldBe("UTC+8");
    }

    [Fact]
    public void GetProperty_ShouldConvertJsonElementsToTypedValues()
    {
        var workspaceSettings = new WorkspaceSettings();
        workspaceSettings.SetProperty("maxUsers", JsonSerializer.SerializeToElement(42));

        var maxUsers = workspaceSettings.GetProperty<int>("maxUsers");

        maxUsers.ShouldBe(42);
    }

    [Fact]
    public void GetProperty_ShouldConvertStringsToGuids()
    {
        var workspaceSettings = new WorkspaceSettings();
        var workspaceId = Guid.NewGuid();
        workspaceSettings.SetProperty("workspaceId", workspaceId.ToString());

        workspaceSettings.GetProperty<Guid>("workspaceId").ShouldBe(workspaceId);
    }

    [Fact]
    public void RemoveProperty_ShouldDeleteTheProperty()
    {
        var workspaceSettings = new WorkspaceSettings();
        workspaceSettings.SetProperty("theme", "light");

        var removed = workspaceSettings.RemoveProperty("theme");

        removed.ShouldBeTrue();
        workspaceSettings.HasProperty("theme").ShouldBeFalse();
    }

    [Fact]
    public void GetProperty_ShouldReturnDefaultWhenMissing()
    {
        var workspaceSettings = new WorkspaceSettings();

        workspaceSettings.GetProperty("timezone", "UTC").ShouldBe("UTC");
    }

    [Fact]
    public void DefinedProperties_ShouldApplyDefaultValuesAndMetadata()
    {
        var workspaceSettings = new WorkspaceSettings();

        workspaceSettings.ApplyDefaults(PropertyManager);

        workspaceSettings.GetProperty<string>("timezone", PropertyManager).ShouldBe("UTC");

        var timezoneDefinition = PropertyManager.FindProperty<WorkspaceSettings>("timezone");

        timezoneDefinition.ShouldNotBeNull();
        timezoneDefinition.DisplayName.ShouldBe("Timezone");
        timezoneDefinition.TryGetMetadata("scope", out var scope).ShouldBeTrue();
        scope.ShouldBe("user");
    }

    [Fact]
    public void SetProperty_WithDefinitionManager_ShouldRejectIncompatibleValues()
    {
        var workspaceSettings = new WorkspaceSettings();

        Should.Throw<InvalidOperationException>(() => workspaceSettings.SetProperty("maxUsers", new object(), PropertyManager));
    }

    [Fact]
    public void ValidateProperties_ShouldRequireDefinedRequiredProperties()
    {
        var workspaceSettings = new WorkspaceSettings();

        Should.Throw<InvalidOperationException>(() => workspaceSettings.ValidateProperties(PropertyManager));

        workspaceSettings.SetProperty("region", "cn-shanghai", PropertyManager);
        workspaceSettings.ValidateProperties(PropertyManager).ShouldBeSameAs(workspaceSettings);
    }

    [Fact]
    public void DefinitionDefault_ShouldBeReturnedWhenPropertyIsMissing()
    {
        var workspaceSettings = new WorkspaceSettings();

        workspaceSettings.GetProperty<string>("timezone", PropertyManager).ShouldBe("UTC");
    }

    private static ExtraPropertyManager CreatePropertyManager()
    {
        var manager = new ExtraPropertyManager();

        manager.AddOrUpdate<WorkspaceSettings, string>("timezone")
            .WithDefaultValue("UTC")
            .WithDisplayName("Timezone")
            .WithMetadata("scope", "user");

        manager.AddOrUpdate<WorkspaceSettings, int>("maxUsers")
            .WithDefaultValue(10)
            .WithMetadata("category", "quota");

        manager.AddOrUpdate<WorkspaceSettings, string>("region")
            .Required();

        return manager;
    }

    private sealed class WorkspaceSettings : IHasExtraProperties
    {
        public ExtraPropertyDictionary ExtraProperties { get; } = new();
    }
}
