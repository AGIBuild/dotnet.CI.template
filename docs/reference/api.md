# API Reference

## Current Public Surface

### `ChengYuan.Core`

Foundational result, identity, and DDD primitives.

#### Key Types

| Type | Description |
|---|---|
| `Result` and `Result<TValue>` | Shared success and failure model for application and framework code. |
| `ResultError` | Stable error shape with code, message, and failure category. |
| `CoreRuntimeModule` | Base runtime module that registers shared Core services such as the extra-property definition registry. |
| `ErrorCode` | Small value object for stable framework and business error codes. |
| `ChengYuanException` | Base exception for framework and application layers with optional error code metadata. |
| `BusinessException` | Exception type that bridges business failures to the shared `ResultError` shape. |
| `Check` | Central guard helper for null, default, range, guid, and collection validation. |
| `StronglyTypedId<TValue>` | Base type for type-safe identifiers used across the framework. |
| `Entity<TId>` | Identity-based base class for entities. |
| `AggregateRoot<TId>` | Entity base class that also tracks domain events. |
| `ValueObject` | Structural equality base class for immutable value semantics. |
| `IHasExtraProperties` | Contract for entities or models that expose an extra property bag. |
| `ExtraPropertyDictionary` | String-keyed property bag used by the object extension model. |
| `ExtraPropertyExtensions` | Typed access helpers for reading and writing extra properties. |
| `ExtraPropertyDefinition` | Named extension-property definition with type, default value, required flag, and metadata. |
| `ExtraPropertyDefinitionBuilder` | Fluent builder for configuring extension-property defaults and metadata. |
| `ExtraPropertyManager` | Registry for extension-property definitions resolved by runtime type. |
| `AddCoreRuntime()` | Registers default Core runtime services into the DI container. |
| `IDomainEvent` | Marker abstraction for aggregate domain events. |
| `IClock` | Time abstraction consumed by runtime and infrastructure code. |
| `IGuidGenerator` | Abstraction for framework-controlled guid generation. |

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

### `ChengYuan.TenantManagement`

Application-layer tenant catalog management.

#### Key Types

| Type | Description |
|---|---|
| `TenantManagementModule` | Registers tenant management services on top of the existing multi-tenancy runtime module. |
| `TenantRecord` | Immutable tenant catalog record carrying id, name, and active state. |
| `ITenantStore` | Stores and queries managed tenants. |
| `ITenantReader` | Exposes read access to managed tenants by id, name, or list. |
| `ITenantManager` | Application-facing service for creating, updating, and removing tenants. |
| `InMemoryTenantStore` | In-memory tenant catalog store for tests and lightweight hosts. |
| `AddInMemoryTenantManagement()` | Registers the in-memory tenant store as both store and reader. |

### `ChengYuan.TenantManagement.Persistence`

Persistence facet for tenant catalog management backed by EF Core.

#### Key Types

| Type | Description |
|---|---|
| `TenantManagementPersistenceModule` | Registers the EF-backed tenant store on top of the TenantManagement application module. |
| `TenantManagementDbContext` | Independent module DbContext for tenant catalog tables within the shared physical database. |
| `TenantEntity` | EF-mapped tenant persistence model with normalized name and soft-delete state. |
| `EfTenantStore` | EF Core implementation of `ITenantStore`. |
| `AddTenantManagementPersistenceDbContext()` | Registers the module DbContext against a caller-provided provider and connection. |
| `AddTenantManagementPersistence()` | Registers the EF-backed tenant store and unit of work services. |

### `ChengYuan.Identity`

First full application bounded context for user management.

#### Key Types

| Type | Description |
|---|---|
| `IdentityModule` | Registers the Identity application services on top of the core runtime module. |
| `UserRecord` | Immutable user snapshot exposed through application contracts. |
| `IUserReader` | Reads users by id, user name, email, or ordered list. |
| `IUserManager` | Creates, updates, and removes users while enforcing unique user name and email rules. |
| `IdentityUser` | Aggregate root for users with normalized user name and email plus soft-delete behavior. |
| `IIdentityUserRepository` | Domain repository contract for identity-user lookup and persistence. |
| `UserManager` | Application service that coordinates uniqueness checks and persistence. |

### `ChengYuan.Identity.Persistence`

Persistence facet for identity-user storage backed by EF Core.

#### Key Types

| Type | Description |
|---|---|
| `IdentityPersistenceModule` | Registers the EF-backed identity-user repository on top of the Identity application module. |
| `IdentityDbContext` | Independent module DbContext for identity-user tables within the shared physical database. |
| `IdentityUserConfiguration` | EF Core model configuration for the identity-user aggregate. |
| `EfIdentityUserRepository` | EF Core implementation of `IIdentityUserRepository`. |
| `AddIdentityDbContext()` | Registers the module DbContext against a caller-provided provider and connection. |
| `AddIdentityPersistence()` | Registers the EF-backed identity repository and unit of work services. |

### `ChengYuan.Authorization`

Definition-driven runtime permission checking.

#### Key Types

| Type | Description |
|---|---|
| `AuthorizationModule` | Registers the default permission definition manager and permission checking pipeline into the service collection. |
| `PermissionScope` | Declares whether a permission grant comes from the global, tenant, or user layer. |
| `PermissionDefinition` | Defines a permission with default grant behavior plus display metadata and description. |
| `IPermissionDefinitionManager` | Registers and resolves named permission definitions. |
| `IPermissionChecker` | Evaluates whether a permission is granted against the ordered provider pipeline. |
| `IPermissionGrantProvider` | Contributes permission grants for specific sources such as user, tenant, or global defaults. |
| `InMemoryPermissionsBuilder` | Fluent builder for preloading global, tenant, and user permission grants into the in-memory providers. |
| `AddInMemoryPermissions()` | Registers host-level in-memory permission providers for tests and simple bootstrapping. |
| `PermissionContext` | Carries the current tenant, user, correlation, and authentication state into permission evaluation. |
| `PermissionGrant` | Carries a resolved grant decision plus its provider source. |

### `ChengYuan.Auditing`

Ambient audit scopes and sink-based audit capture.

#### Key Types

| Type | Description |
|---|---|
| `AuditingModule` | Registers the default audit scope factory into the service collection. |
| `AuditLogEntry` | Mutable audit payload carrying timing, ambient context, success state, and custom properties. |
| `IAuditScopeFactory` | Creates ambient audit scopes and exposes helper methods for wrapping async operations. |
| `IAuditScope` | Represents a live audit scope that can mark success or failure and attach custom properties. |
| `IAuditLogContributor` | Enriches audit entries before they are dispatched to sinks. |
| `IAuditLogSink` | Receives completed audit entries for persistence, export, or diagnostics. |
| `InMemoryAuditLogCollector` | Stores completed audit entries in memory for tests and lightweight hosts. |
| `AddInMemoryAuditing()` | Registers an in-memory audit sink backed by the collector service. |

### `ChengYuan.AuditLogging`

Application-layer audit log persistence and retrieval.

#### Key Types

| Type | Description |
|---|---|
| `AuditLoggingModule` | Connects framework audit sinks to an application-layer audit log store. |
| `AuditLogRecord` | Immutable audit log snapshot stored by application-layer stores. |
| `IAuditLogStore` | Appends and lists persisted audit log records. |
| `IAuditLogReader` | Exposes read access to stored audit log records. |
| `InMemoryAuditLogStore` | In-memory application store for tests and lightweight hosts. |
| `AddInMemoryAuditLogging()` | Registers the in-memory audit log store as both store and reader. |

### `ChengYuan.AuditLogging.Persistence`

Persistence facet for audit log storage backed by EF Core.

#### Key Types

| Type | Description |
|---|---|
| `AuditLoggingPersistenceModule` | Registers the EF-backed audit log store on top of the AuditLogging application module. |
| `AuditLoggingDbContext` | Independent module DbContext for audit log tables within the shared physical database. |
| `AuditLogEntity` | EF-mapped audit log persistence model including serialized custom properties. |
| `EfAuditLogStore` | EF Core implementation of `IAuditLogStore`. |
| `AddAuditLoggingPersistenceDbContext()` | Registers the module DbContext and DbContext factory against a caller-provided provider and connection. |
| `AddAuditLoggingPersistence()` | Registers the EF-backed audit log store and unit of work services. |

### `ChengYuan.FeatureManagement`

Application-layer feature value storage and management.

#### Key Types

| Type | Description |
|---|---|
| `FeatureManagementModule` | Connects application-layer feature value management to framework feature providers. |
| `FeatureValueRecord` | Immutable feature record stored per global, tenant, or user scope. |
| `IFeatureValueStore` | Stores and queries feature values used by the application module. |
| `IFeatureValueReader` | Exposes read access to stored feature values. |
| `IFeatureValueManager` | Application-facing service for setting and removing values. |
| `InMemoryFeatureValueStore` | In-memory feature value store for tests and lightweight hosts. |
| `AddInMemoryFeatureManagement()` | Registers the in-memory feature store as both store and reader. |

### `ChengYuan.FeatureManagement.Persistence`

Persistence facet for feature value storage backed by EF Core.

#### Key Types

| Type | Description |
|---|---|
| `FeatureManagementPersistenceModule` | Registers the EF-backed feature value store on top of the FeatureManagement application module. |
| `FeatureManagementDbContext` | Independent module DbContext for feature value tables within the shared physical database. |
| `FeatureValueEntity` | EF-mapped feature value persistence model for global, tenant, and user scopes. |
| `EfFeatureValueStore` | EF Core implementation of `IFeatureValueStore`. |
| `AddFeatureManagementPersistenceDbContext()` | Registers the module DbContext and DbContext factory against a caller-provided provider and connection. |
| `AddFeatureManagementPersistence()` | Registers the EF-backed feature value store and unit of work services. |

### `ChengYuan.PermissionManagement`

Application-layer permission grant storage and management.

#### Key Types

| Type | Description |
|---|---|
| `PermissionManagementModule` | Connects application-layer permission grant management to framework authorization providers. |
| `PermissionGrantRecord` | Immutable grant record stored per global, tenant, or user scope. |
| `IPermissionGrantStore` | Stores and queries permission grants used by the application module. |
| `IPermissionGrantReader` | Exposes read access to stored permission grants. |
| `IPermissionGrantManager` | Application-facing service for setting and removing permission grants. |
| `InMemoryPermissionGrantStore` | In-memory permission grant store for tests and lightweight hosts. |
| `AddInMemoryPermissionManagement()` | Registers the in-memory grant store as both store and reader. |

### `ChengYuan.PermissionManagement.Persistence`

Persistence facet for permission grant storage backed by EF Core.

#### Key Types

| Type | Description |
|---|---|
| `PermissionManagementPersistenceModule` | Registers the EF-backed permission grant store on top of the PermissionManagement application module. |
| `PermissionManagementDbContext` | Independent module DbContext for permission grant tables within the shared physical database. |
| `PermissionGrantEntity` | EF-mapped permission grant persistence model for global, tenant, and user scopes. |
| `EfPermissionGrantStore` | EF Core implementation of `IPermissionGrantStore`. |
| `AddPermissionManagementPersistenceDbContext()` | Registers the module DbContext and DbContext factory against a caller-provided provider and connection. |
| `AddPermissionManagementPersistence()` | Registers the EF-backed permission grant store and unit of work services. |

### `ChengYuan.SettingManagement`

Application-layer setting storage and management.

#### Key Types

| Type | Description |
|---|---|
| `SettingManagementModule` | Connects application-layer setting value management to framework setting providers. |
| `SettingValueRecord` | Immutable setting record stored per global, tenant, or user scope. |
| `ISettingValueStore` | Stores and queries setting values used by the application module. |
| `ISettingValueReader` | Exposes read access to stored setting values. |
| `ISettingValueManager` | Application-facing service for setting and removing values. |
| `InMemorySettingValueStore` | In-memory setting value store for tests and lightweight hosts. |
| `AddInMemorySettingManagement()` | Registers the in-memory setting store as both store and reader. |

### `ChengYuan.SettingManagement.Persistence`

Persistence facet for setting value storage backed by EF Core.

#### Key Types

| Type | Description |
|---|---|
| `SettingManagementPersistenceModule` | Registers the EF-backed setting value store on top of the SettingManagement application module. |
| `SettingManagementDbContext` | Independent module DbContext for setting value tables within the shared physical database. |
| `SettingValueEntity` | EF-mapped setting value persistence model for global, tenant, and user scopes. |
| `EfSettingValueStore` | EF Core implementation of `ISettingValueStore`. |
| `AddSettingManagementPersistenceDbContext()` | Registers the module DbContext and DbContext factory against a caller-provided provider and connection. |
| `AddSettingManagementPersistence()` | Registers the EF-backed setting store and unit of work services. |

### `ChengYuan.Settings`

Definition-driven runtime settings resolution.

#### Key Types

| Type | Description |
|---|---|
| `SettingsModule` | Registers the default settings definition manager and resolution pipeline into the service collection. |
| `SettingScope` | Declares whether a setting value comes from the global, tenant, or user layer. |
| `SettingDefinition` | Defines a typed setting with default value, display metadata, and description. |
| `ISettingDefinitionManager` | Registers and resolves named setting definitions. |
| `ISettingProvider` | Resolves typed setting values by applying the ordered provider pipeline against the current user and tenant context. |
| `ISettingValueProvider` | Contributes setting values for a specific source such as user, tenant, or global defaults. |
| `InMemorySettingsBuilder` | Fluent builder for preloading global, tenant, and user setting values into the in-memory providers. |
| `AddInMemorySettings()` | Registers host-level in-memory setting value providers for testing and simple bootstrapping. |
| `SettingContext` | Carries the current tenant, user, and correlation values into setting resolution. |
| `SettingValue` | Carries a resolved raw value plus its provider source. |

### `ChengYuan.Features`

Definition-driven runtime feature evaluation.

#### Key Types

| Type | Description |
|---|---|
| `FeaturesModule` | Registers the default feature definition manager and evaluation pipeline into the service collection. |
| `FeatureScope` | Declares whether a feature value comes from the global, tenant, or user layer. |
| `FeatureDefinition` | Defines a typed feature with default value, display metadata, and description. |
| `IFeatureDefinitionManager` | Registers and resolves named feature definitions. |
| `IFeatureChecker` | Resolves typed feature values and exposes boolean enablement checks against the ordered provider pipeline. |
| `IFeatureValueProvider` | Contributes feature values for a specific source such as user, tenant, or global defaults. |
| `InMemoryFeaturesBuilder` | Fluent builder for preloading global, tenant, and user feature values into the in-memory providers. |
| `AddInMemoryFeatures()` | Registers host-level in-memory feature value providers for testing and simple bootstrapping. |
| `FeatureContext` | Carries the current tenant, user, and correlation values into feature evaluation. |
| `FeatureValue` | Carries a resolved raw value plus its provider source. |

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
