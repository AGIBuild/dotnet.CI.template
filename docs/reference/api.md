# API Reference

## Current Public Surface

### `ChengYuan.Core`

Foundational result, identity, and DDD primitives.

#### Key Types

| Type | Description |
|---|---|
| `Result` and `Result<TValue>` | Shared success and failure model for application and framework code. |
| `ResultError` | Stable error shape with code, message, and failure category. |
| `StronglyTypedId<TValue>` | Base type for type-safe identifiers used across the framework. |
| `Entity<TId>` | Identity-based base class for entities. |
| `AggregateRoot<TId>` | Entity base class that also tracks domain events. |
| `ValueObject` | Structural equality base class for immutable value semantics. |
| `IDomainEvent` | Marker abstraction for aggregate domain events. |
| `IClock` | Time abstraction consumed by runtime and infrastructure code. |

### `ChengYuan.Core.Runtime`

Core module composition primitives.

#### Key Types

| Type | Description |
|---|---|---|
| `ModuleBase` | Base type for framework and application modules. |
| `DependsOnAttribute` | Declares direct module dependencies for composition ordering. |
| `ModuleCatalog` | Exposes the ordered module list loaded by a host. |

### `ChengYuan.Core.Json`

Core Json integration for foundational primitives.

#### Key Types

| Type | Description |
|---|---|
| `JsonModule` | Optional module that hosts Json-related core integration. |
| `StronglyTypedIdJsonConverterFactory` | Registers strongly typed ID converters for System.Text.Json. |

### `ChengYuan.Core.Validation`

Default validation pipeline for core-level contracts.

#### Key Types

| Type | Description |
|---|---|
| `ValidationModule` | Registers the default object validator into the service collection. |
| `IValidationRule<TValue>` | Contributes validation errors for a specific input model. |
| `IObjectValidator<TValue>` | Aggregates all rules for a given input and returns a validation result. |
| `ValidationResult` | Carries validation errors without forcing a transport or persistence dependency. |

### `ChengYuan.Core.Localization`

Error and resource localization seams for core contracts.

#### Key Types

| Type | Description |
|---|---|
| `LocalizationModule` | Registers the default error localizer into the service collection. |
| `ILocalizationResource` | Resource seam for resolving localized strings by error code. |
| `IErrorLocalizer` | Localizes a `ResultError` with resource-first and fallback behavior. |

### `ChengYuan.Core.Data`

Provider-agnostic data contracts for repositories, unit of work, and filter seams.

#### Key Types

| Type | Description |
|---|---|
| `DataModule` | Registers the default open-generic data filter implementation. |
| `IDataFilter<TFilter>` | Ambient filter seam that can be enabled or disabled per async flow. |
| `SoftDeleteFilter` | Marker filter for soft-delete aware data providers. |
| `MultiTenantFilter` | Marker filter for tenant-aware data providers. |
| `IDataTenantProvider` | Provider-agnostic seam for exposing the current tenant id to data infrastructure. |
| `ISoftDelete` | Persistence-facing trait for entities that can be soft deleted. |
| `IMultiTenant` | Persistence-facing trait for entities scoped by tenant id. |
| `IReadOnlyRepository<TEntity, TId>` | Provider-agnostic read contract for aggregate root retrieval. |
| `IRepository<TEntity, TId>` | Minimal mutation contract built on the aggregate root read seam. |
| `IUnitOfWork` | Contract for deferring persistence commits to a provider implementation. |
| `IUnitOfWorkAccessor` | Ambient accessor used by provider implementations to flow the current unit of work. |

### `ChengYuan.Core.EntityFrameworkCore`

EF Core integration for foundational primitives.

#### Key Types

| Type | Description |
|---|---|
| `StronglyTypedIdValueConverter<TStronglyTypedId, TValue>` | Converts strongly typed IDs to and from provider values. |
| `DbContextUnitOfWork` | Adapts an EF Core `DbContext` to the provider-agnostic `IUnitOfWork` contract. |
| `EfRepository<TDbContext, TEntity, TId>` | Provider implementation of the aggregate root repository contract on top of a `DbContext`, including soft-delete and tenant-aware queries when the corresponding filters are enabled. |
| `EntityFrameworkCoreServiceCollectionExtensions` | Registers EF-backed unit of work and repository services into DI for a specific `DbContext`. |

### `ChengYuan.ExecutionContext.Abstractions`

Ambient execution context contracts.

#### Key Types

| Type | Description |
|---|---|
| `ICurrentUser` | Read-only view of the current user identity. |
| `ICurrentUserAccessor` | Scope-based accessor used by transports and infrastructure. |
| `ICurrentCorrelation` | Access to the current correlation identifier. |
| `ICurrentCorrelationAccessor` | Scope-based correlation accessor. |
| `IClock` | Time abstraction for application and framework code. |

### `ChengYuan.MultiTenancy.Abstractions`

Ambient tenant context contracts.

#### Key Types

| Type | Description |
|---|---|
| `ICurrentTenant` | Read-only view of the active tenant context. |
| `ICurrentTenantAccessor` | Scope-based accessor for tenant switching. |
| `TenantInfo` | Immutable tenant snapshot exposed to consumers. |

#### Example

```csharp
builder.Services.AddModule<WebHostModule>();

options.Converters.Add(new StronglyTypedIdJsonConverterFactory());

var validationResult = validator.Validate(command);
var errorMessage = errorLocalizer.Localize(validationResult.Errors[0]);

using (softDeleteFilter.Disable())
{
	var deletedWorkspace = await repository.FindAsync(workspaceId, cancellationToken);
}

using (unitOfWorkAccessor.Change(unitOfWork))
{
	await unitOfWorkAccessor.Current!.SaveChangesAsync(cancellationToken);
}
```

---

> **Tip for template users**: Expand this page module by module as contracts stabilize. Consider using [DocFX](https://dotnet.github.io/docfx/) or [xmldoc2md](https://github.com/nicolestandifer3/xmldoc2md-csharp) for generated API references from XML comments.
