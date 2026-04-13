using System.Text.Json;
using ChengYuan.Core.Entities;
using ChengYuan.Core.Exceptions;
using ChengYuan.Core.Json;
using ChengYuan.Core.Modularity;
using ChengYuan.Core.Results;
using ChengYuan.Core.StronglyTypedIds;
using ChengYuan.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class DomainPrimitivesTests
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = CreateJsonSerializerOptions();

    [Fact]
    public void JsonModule_ShouldLoadIntoTheServiceCollection()
    {
        var services = new ServiceCollection();
        services.AddModule<JsonTestModule>();

        using var serviceProvider = services.BuildServiceProvider();
        var moduleCatalog = serviceProvider.GetRequiredService<ModuleCatalog>();

        moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ShouldContain(nameof(JsonModule));
    }

    [Fact]
    public void Result_ShouldRepresentSuccessWithoutError()
    {
        var result = Result.Success();

        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBe(ResultError.None);
    }

    [Fact]
    public void Result_ShouldRepresentFailureWithError()
    {
        var error = ResultError.Validation("workspace.name.required", "Workspace name is required.");
        var result = Result.Failure(error);

        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(error);
        result.Error.Type.ShouldBe(ResultErrorType.Validation);
    }

    [Fact]
    public void GenericResult_ShouldMapSuccessfulValues()
    {
        var result = Result.Success(42);
        var mapped = result.Map(value => $"workspace-{value}");

        mapped.IsSuccess.ShouldBeTrue();
        mapped.Value.ShouldBe("workspace-42");
        mapped.Error.ShouldBe(ResultError.None);
    }

    [Fact]
    public void GenericResult_ShouldPreserveFailuresWhenMapped()
    {
        var error = ResultError.NotFound("workspace.not-found", "Workspace was not found.");
        var result = Result.Failure<int>(error);
        var mapped = result.Map(value => $"workspace-{value}");

        mapped.IsFailure.ShouldBeTrue();
        mapped.Error.ShouldBe(error);
        Should.Throw<InvalidOperationException>(() => _ = mapped.Value);
    }

    [Fact]
    public void ErrorCode_ShouldRejectWhitespaceValues()
    {
        Should.Throw<ArgumentException>(() => _ = new ErrorCode(" "));
    }

    [Fact]
    public void BusinessException_ShouldExposeErrorMetadata()
    {
        var exception = new BusinessException(
            "Workspace was archived.",
            new ErrorCode("workspace.archived"),
            ResultErrorType.Conflict);

        exception.ErrorCode.ShouldBe(new ErrorCode("workspace.archived"));
        exception.ErrorType.ShouldBe(ResultErrorType.Conflict);
        exception.ToResultError().ShouldBe(ResultError.Conflict("workspace.archived", "Workspace was archived."));
    }

    [Fact]
    public void ChengYuanException_ShouldPreserveInnerExceptionAndOptionalCode()
    {
        var innerException = new InvalidOperationException("Inner");
        var exception = new ChengYuanException("Workspace failed.", new ErrorCode("workspace.failed"), innerException);

        exception.Message.ShouldBe("Workspace failed.");
        exception.ErrorCode.ShouldBe(new ErrorCode("workspace.failed"));
        exception.InnerException.ShouldBeSameAs(innerException);
    }

    [Fact]
    public void GuidStronglyTypedId_ShouldProvideTypeSafeValueEquality()
    {
        var value = Guid.NewGuid();
        var workspaceId = new WorkspaceId(value);
        var sameWorkspaceId = new WorkspaceId(value);
        var userId = new UserId(value);

        workspaceId.ShouldBe(sameWorkspaceId);
        workspaceId.Equals(userId).ShouldBeFalse();
        workspaceId.Value.ShouldBe(value);
        workspaceId.ValueText.ShouldBe(value.ToString());
    }

    [Fact]
    public void GuidStronglyTypedId_ShouldRejectEmptyGuid()
    {
        Should.Throw<ArgumentException>(() => new WorkspaceId(Guid.Empty));
    }

    [Fact]
    public void StronglyTypedIdJsonConverterFactory_ShouldSerializeAsUnderlyingValue()
    {
        var value = Guid.NewGuid();
        var workspaceId = new WorkspaceId(value);

        var json = JsonSerializer.Serialize(workspaceId, JsonSerializerOptions);

        json.ShouldBe($"\"{value}\"");
    }

    [Fact]
    public void StronglyTypedIdJsonConverterFactory_ShouldRoundTripStronglyTypedIds()
    {
        var value = Guid.NewGuid();

        var workspaceId = JsonSerializer.Deserialize<WorkspaceId>($"\"{value}\"", JsonSerializerOptions);

        workspaceId.ShouldNotBeNull();
        workspaceId.Value.ShouldBe(value);
    }

    [Fact]
    public void StronglyTypedIdActivator_ShouldCreateStronglyTypedIdsFromUnderlyingValues()
    {
        var value = Guid.NewGuid();

        var workspaceId = StronglyTypedIdActivator.Create<WorkspaceId, Guid>(value);

        workspaceId.Value.ShouldBe(value);
    }

    [Fact]
    public void StronglyTypedIdValueConverter_ShouldConvertBetweenStronglyTypedIdsAndProviderValues()
    {
        var value = Guid.NewGuid();
        var workspaceId = new WorkspaceId(value);
        var converter = new StronglyTypedIdValueConverter<WorkspaceId, Guid>();

        var providerValue = converter.ConvertToProviderExpression.Compile()(workspaceId);
        var materializedId = converter.ConvertFromProviderExpression.Compile()(value);

        providerValue.ShouldBe(value);
        materializedId.Value.ShouldBe(value);
    }

    [Fact]
    public void Entity_ShouldUseIdentityEqualityPerConcreteType()
    {
        var value = Guid.NewGuid();
        var workspaceId = new WorkspaceId(value);
        var sameWorkspace = new Workspace(workspaceId);
        var sameWorkspaceWithSameId = new Workspace(new WorkspaceId(value));
        var otherAggregateType = new WorkspaceMember(new WorkspaceId(value));

        sameWorkspace.ShouldBe(sameWorkspaceWithSameId);
        sameWorkspace.Equals(otherAggregateType).ShouldBeFalse();
        sameWorkspace.Id.ShouldBe(workspaceId);
    }

    [Fact]
    public void AggregateRoot_ShouldTrackAndClearDomainEvents()
    {
        var workspace = new Workspace(new WorkspaceId(Guid.NewGuid()));

        workspace.Archive();

        workspace.DomainEvents.Count.ShouldBe(1);
        workspace.DomainEvents.First().ShouldBeOfType<WorkspaceArchived>();

        workspace.ClearDomainEvents();

        workspace.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public void ValueObject_ShouldUseStructuralEquality()
    {
        var slug = new WorkspaceSlug("core", "runtime");
        var sameSlug = new WorkspaceSlug("core", "runtime");
        var otherSlug = new WorkspaceSlug("core", "data");

        slug.ShouldBe(sameSlug);
        slug.ShouldNotBe(otherSlug);
    }

    [DependsOn(typeof(JsonModule))]
    private sealed class JsonTestModule : FrameworkCoreModule
    {
    }

    private sealed record WorkspaceId : GuidStronglyTypedId
    {
        public WorkspaceId(Guid value)
            : base(value)
        {
        }
    }

    private sealed record UserId : GuidStronglyTypedId
    {
        public UserId(Guid value)
            : base(value)
        {
        }
    }

    private sealed class Workspace : AggregateRoot<WorkspaceId>
    {
        public Workspace(WorkspaceId id)
            : base(id)
        {
        }

        public void Archive()
        {
            AddDomainEvent(new WorkspaceArchived(Id));
        }
    }

    private sealed class WorkspaceMember : Entity<WorkspaceId>
    {
        public WorkspaceMember(WorkspaceId id)
            : base(id)
        {
        }
    }

    private sealed class WorkspaceSlug(string area, string name) : ValueObject
    {
        private readonly string _area = area;
        private readonly string _name = name;

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return _area;
            yield return _name;
        }
    }

    private sealed record WorkspaceArchived(WorkspaceId WorkspaceId) : IDomainEvent;

    private static JsonSerializerOptions CreateJsonSerializerOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new StronglyTypedIdJsonConverterFactory());
        return options;
    }
}
