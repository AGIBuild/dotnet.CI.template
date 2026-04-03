using System.Globalization;
using ChengYuan.Core.Localization;
using ChengYuan.Core.Modularity;
using ChengYuan.Core.Results;
using ChengYuan.Core.Validation;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class CoreSupportModuleTests
{
    [Fact]
    public void ValidationModule_ShouldRegisterDefaultObjectValidator()
    {
        var services = new ServiceCollection();
        services.AddModule<ValidationTestModule>();
        services.AddTransient<IValidationRule<CreateWorkspaceCommand>, WorkspaceNameRequiredRule>();
        services.AddTransient<IValidationRule<CreateWorkspaceCommand>, WorkspaceDescriptionRequiredRule>();

        using var serviceProvider = services.BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<IObjectValidator<CreateWorkspaceCommand>>();

        var result = validator.Validate(new CreateWorkspaceCommand(string.Empty, string.Empty));

        result.IsInvalid.ShouldBeTrue();
        result.Errors.Count.ShouldBe(2);
        result.ToResult().Error.Code.ShouldBe("workspace.name.required");
    }

    [Fact]
    public void LocalizationModule_ShouldResolveRegisteredResourceStrings()
    {
        var services = new ServiceCollection();
        services.AddModule<LocalizationTestModule>();
        services.AddSingleton<ILocalizationResource>(
            new DictionaryLocalizationResource(
                "Core",
                new Dictionary<string, string>
                {
                    ["workspace.name.required"] = "Workspace name is required from the resource."
                }));

        using var serviceProvider = services.BuildServiceProvider();
        var localizer = serviceProvider.GetRequiredService<IErrorLocalizer>();

        var localizedText = localizer.Localize(
            ResultError.Validation("workspace.name.required", "Fallback message."),
            CultureInfo.InvariantCulture);

        localizedText.ShouldBe("Workspace name is required from the resource.");
    }

    [Fact]
    public void LocalizationModule_ShouldFallbackToTheOriginalErrorMessage()
    {
        var services = new ServiceCollection();
        services.AddModule<LocalizationTestModule>();

        using var serviceProvider = services.BuildServiceProvider();
        var localizer = serviceProvider.GetRequiredService<IErrorLocalizer>();

        var localizedText = localizer.Localize(
            ResultError.Validation("workspace.slug.required", "Fallback message."),
            CultureInfo.InvariantCulture);

        localizedText.ShouldBe("Fallback message.");
    }

    [DependsOn(typeof(ValidationModule))]
    private sealed class ValidationTestModule : ModuleBase
    {
    }

    [DependsOn(typeof(LocalizationModule))]
    private sealed class LocalizationTestModule : ModuleBase
    {
    }

    private sealed record CreateWorkspaceCommand(string Name, string Description);

    private sealed class WorkspaceNameRequiredRule : IValidationRule<CreateWorkspaceCommand>
    {
        public IEnumerable<ResultError> Validate(CreateWorkspaceCommand value)
        {
            if (!string.IsNullOrWhiteSpace(value.Name))
            {
                return [];
            }

            return [ResultError.Validation("workspace.name.required", "Workspace name is required.")];
        }
    }

    private sealed class WorkspaceDescriptionRequiredRule : IValidationRule<CreateWorkspaceCommand>
    {
        public IEnumerable<ResultError> Validate(CreateWorkspaceCommand value)
        {
            if (!string.IsNullOrWhiteSpace(value.Description))
            {
                return [];
            }

            return [ResultError.Validation("workspace.description.required", "Workspace description is required.")];
        }
    }

    private sealed class DictionaryLocalizationResource(string name, IReadOnlyDictionary<string, string> values) : ILocalizationResource
    {
        public string Name { get; } = name;

        public bool TryGetString(string key, CultureInfo culture, out string? value)
        {
            ArgumentNullException.ThrowIfNull(culture);
            return values.TryGetValue(key, out value);
        }
    }
}
