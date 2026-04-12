# Architecture

This page is the authoritative architecture guide for ChengYuan.

When existing code and this guide differ, use this guide as the default rule for new work and move the codebase toward these rules instead of extending the divergence.

## Design Principles

- Module first, layer second.
- Keep the logical taxonomy explicit: `FrameworkCore`, `Application`, `Extension`, and `Host`.
- Split reusable slices so technical systems stay separate from reusable application capabilities and optional extensions.
- Allow uneven module depth. Do not force every module to expose the same layers or transports.
- Keep Web and CLI as optional transport facets.
- Require explicit dependencies and architecture tests.
- Preserve strong engineering guardrails: build automation, locked restore, versioning, tests, and documentation.

## Top-Level Structure

```text
src/
├── Framework/      # Technical modules and provider integrations
├── Applications/   # Reusable application modules and later business modules
├── Hosts/          # Runnable shells such as WebHost and CliHost
├── ...
tests/              # Unit, integration, architecture, and template smoke tests
build/              # NUKE build and packaging logic
templates/          # dotnet new template assets
docs/               # Documentation and contributor guidance
```

Use one level of module nesting inside the module families:

```text
src/
├── Framework/
│   └── Caching/
│       ├── ChengYuan.Caching.Abstractions/
│       ├── ChengYuan.Caching.Runtime/
│       └── ChengYuan.Caching.Redis/
├── Applications/
│   └── Identity/
│       ├── ChengYuan.Identity.Contracts/
│       ├── ChengYuan.Identity.Domain/
│       ├── ChengYuan.Identity.Application/
│       ├── ChengYuan.Identity.Persistence/
│       ├── ChengYuan.Identity.Web/
│       └── ChengYuan.Identity.Cli/
└── Hosts/
    ├── WebHost/
    └── CliHost/
```

The preferred path shape is `family root -> short module folder -> project`. Use `src/Applications/Identity/ChengYuan.Identity.Application/`, not `src/Applications/ChengYuan.Identity/ChengYuan.Identity.Application/`, and do not add role-only intermediate folders such as `src/Applications/Identity/Application/`.

Apply the same mirroring rule to tests. Keep module tests under paths such as `tests/ChengYuan.FrameworkKernel.Tests/Applications/Identity/` and architecture suites under paths such as `tests/ChengYuan.ArchitectureTests/Structure/`.

Family words such as `Framework`, `Applications`, and `Hosts` belong in directories, not in project names. Facet words such as `Application`, `Persistence`, and `Web` stay in project names when they describe the project role.

## Logical Taxonomy

ChengYuan currently keeps the physical top-level folders `Framework`, `Applications`, and `Hosts`, but new design work should follow the logical taxonomy below:

| Logical Class | Meaning | Typical Examples |
|---|---|---|
| `FrameworkCore` | Reusable technical capabilities and modularity primitives that are broadly reusable across business modules | `Core`, `ExecutionContext`, `MultiTenancy`, `Authorization`, `Settings`, `Features`, `Auditing`, `Outbox` |
| `Application` | Reusable business capabilities and bounded contexts | `Identity`, `TenantManagement`, `SettingManagement`, `PermissionManagement`, `FeatureManagement`, `AuditLogging` |
| `Extension` | Optional, on-demand modules that connect a capability to a concrete technology or transport | `*.Persistence`, `*.Web`, `MemoryCaching`, future `Redis`, `MongoDb`, HTTP tenant source adapters |
| `Host` | Runnable shells that compose modules and provide environment-specific configuration | `WebHost`, `CliHost` |

The classification is based on responsibility, not directory name. For example, `ChengYuan.Caching.Memory` lives under `src/Framework/`, but its logical class is `Extension`, not `FrameworkCore`.

### Module Base Classes

Each logical class maps to a dedicated abstract base class. Production modules must inherit the corresponding category-specific base class instead of `ModuleBase` directly:

| Logical Class | Base Class | When to Use |
|---|---|---|
| `FrameworkCore` | `FrameworkCoreModule` | Shared technical capabilities and modularity primitives |
| `Application` | `ApplicationModule` | Business capabilities and bounded contexts |
| `Extension` | `ExtensionModule` | Storage, transport, adapter, or technology bindings |
| `Host` | `HostModule` | Runnable composition shells and environment glue |

`ModuleBase` remains the low-level engine primitive. Direct `ModuleBase` inheritance is reserved for low-level modularity infrastructure only. All other modules should choose one of the four category-specific bases.

The service-registration entry point is shared: every module receives a `ServiceConfigurationContext` that wraps `IServiceCollection` along with an `IInitLoggerFactory` for pre-DI logging and an `Items` dictionary for cross-module state. `ModuleBase` provides `Configure<TOptions>()` helpers and owns the shared lifecycle template: `OnLoaded`, `PreConfigureServices`, `ConfigureServices`, `PostConfigureServices`, `PreInitializeAsync`, `InitializeAsync`, `PostInitializeAsync`, and `ShutdownAsync`. During module initialization, any log entries buffered through `IInitLogger<T>` during the service-registration phase are replayed through the real `ILoggerFactory`.

During `OnLoaded`, `ModuleBase` caches the current module descriptor, direct dependencies, direct dependents, resolved category, and root-module state. Category-specific base classes should use this cached topology instead of querying the catalog again.

The category-specific base classes are not markers only. They add category-aware behavior during the load stage and expose category-scoped lifecycle hooks for derived modules:

- `FrameworkCoreModule` validates that framework modules only depend on other `FrameworkCore` modules, then distinguishes shared foundations from leaf foundations based on cached dependents.
- `ApplicationModule` validates that application modules only depend on `FrameworkCore` or other `Application` modules, then exposes capability-owner state such as whether they are extended by `Extension` modules or composed directly by `Host` modules.
- `ExtensionModule` validates that extension modules only depend on `FrameworkCore`, `Application`, or other `Extension` modules, then resolves the capability they attach to from the dependency graph. Persistence remains an `Extension` shape, not a fifth category.
- `HostModule` validates that host modules are only depended on by other `Host` modules, then branches behavior between root startup hosts and inner host-composition modules by using cached root-module state.

`ModuleDescriptor.Category` remains the runtime-visible metadata for inspection, tests, and host diagnostics. New modules should override the template methods or category-scoped hooks directly.

## Module Families

| Family | Purpose | Typical Examples | Notes |
|---|---|---|---|
| `Framework` | Technical systems, abstractions, runtime services, provider models, infrastructure bridges | `Core`, `ExecutionContext`, `Caching`, `Authorization`, `Settings`, `Outbox`, `Hosting` | Should not own user-facing business capabilities |
| `Applications` | Reusable application capabilities and business bounded contexts | `Identity`, `PermissionManagement`, `TenantManagement`, `SettingManagement`, later user business modules | Can own domain behavior, management use cases, persistence, and optional transports |
| `Hosts` | Runnable composition shells | `WebHost`, `CliHost` | Compose modules, policies, middleware, and transport glue only |

`ChengYuan.Core` is the only framework module family member allowed to own foundational modularity, failure, DDD, and shared extension seams. `ChengYuan.Hosting` stays as a thin composition helper and must not own the module model.

Within these physical families, the logical rule is: `FrameworkCore` and `Extension` may both live under `src/Framework/`, while `Application` and its optional `Extension` projects may both live under `src/Applications/`. `Host` projects remain under `src/Hosts/`.

## Facet Model

Every module is a vertical slice, but not every slice needs the same facets.

### Framework Module Facets

| Facet | Responsibility |
|---|---|
| `Abstractions` | Interfaces, options contracts, constants, safe shared types, extension points |
| `Runtime` | Core implementation of the framework capability |
| `Provider` | Replaceable technology integration such as memory, Redis, EF Core, PostgreSQL |
| `AspNetCore` | Optional HTTP or request-pipeline bridge |
| `Worker` | Optional background processing or polling facet |

### Application Module Facets

| Facet | Responsibility |
|---|---|
| `Contracts` | DTOs, application service interfaces, permission names, integration event contracts |
| `Domain` | Aggregates, repository interfaces, domain services, policies, domain events |
| `Application` | Use-case orchestration, validation, application services, transaction boundaries |
| `Persistence` | Storage implementation for stateful modules |
| `Web` | Optional HTTP adapter |
| `Cli` | Optional CLI adapter |

`Persistence`, `Web`, `Cli`, `Memory`, and similar technology-facing facets are treated as `Extension` modules in the logical taxonomy. They are optional modules that can be attached to an `Application` or `FrameworkCore` capability when needed.

### Uneven Depth Is Intentional

Examples:

- `ChengYuan.Caching` can be a shallow framework module with `Abstractions`, `Runtime`, and technology providers only.
- `ChengYuan.Outbox` can expose `Abstractions`, `Runtime`, `Persistence`, and `Worker`, with no Web or CLI facet.
- `ChengYuan.AuditLogging` can begin as a headless application module with `Contracts`, `Application`, and `Persistence`, then add Web or CLI later if needed.
- `ChengYuan.Identity` can be a full application module with `Contracts`, `Domain`, `Application`, `Persistence`, and optional Web or CLI adapters.

Do not create empty projects just to make all modules look symmetrical.

## Module Pairing Rules

The core rule is to separate technical systems from the management modules built on top of them.

| Framework Module | Application Module | Reason |
|---|---|---|
| `Authorization` | `PermissionManagement` | Permission checking is technical; permission grant management is an application capability |
| `Settings` | `SettingManagement` | Setting definitions and reading are technical; runtime editing and persistence are application concerns |
| `MultiTenancy` | `TenantManagement` | Tenant context and resolution are technical; tenant administration is an application capability |
| `Auditing` | `AuditLogging` | Audit scopes and contracts are technical; log persistence and management are application concerns |
| `Features` | `FeatureManagement` | Feature evaluation is technical; feature editing and persistence are application concerns |

`Identity` stays in `Applications` because it owns a reusable application capability with domain behavior, management workflows, and administration surfaces.

## Multi-Tenancy Runtime Rules (Web-First Design)

- `ChengYuan.MultiTenancy` owns tenant context, tenant resolution abstractions, and runtime scope activation.
- `ChengYuan.TenantManagement` owns tenant administration, catalog persistence, and tenant-specific data models. It is optional and must not be referenced by runtime or WebHost composition.
- **Multi-tenancy is currently a Web-first feature.** Framework primitives (ambient tenant context, resolution contracts) work across hosts, but the full resolution source set is Web-only.
- ASP.NET Core hosts activate request-scoped tenant resolution through:
  - `services.AddMultiTenancy(builder => builder.UseHeader(...).UseQueryString(...))` for service registration.
  - `app.UseMultiTenancy()` middleware insertion.
- Built-in Web input sources (in priority order):
  1. Authenticated claims (highest security priority, Order 100)
  2. HTTP headers (Order 200)
  3. Query string (Order 300)
  4. Route values (Order 400)
  5. Cookies (Order 500)
  6. Domain/subdomain extraction (Order 600)
- Each source supports both Guid-based and name-based candidates. Name candidates are normalized through `ITenantResolutionStore` when a store implementation is registered.
- `TenantManagement` now provides the default application-side implementation of `ITenantResolutionStore` for both in-memory and persistence-backed catalogs.
- Resolution outcomes: `Resolved`, `Unresolved`, `NotFound`, `Inactive`. Middleware branches on outcome and supports a configurable error handler delegate.
- Fallback is only applied when **no source provides a candidate**; if a candidate is supplied but lookup fails, the outcome is `NotFound`, not fallback.
- Custom contributors can be registered via `builder.AddContributor<T>()` for semantic resolution.
- Custom sources can be registered via `builder.AddSource<T>()` for host-specific input extraction.
- `WebHost` keeps one thin public composition seam: `AddWebHostComposition(...)` for service registration and `UseWebHostComposition()` for pipeline activation.
- Internally, `WebHost` should be split into three explicit composition modules: framework composition, application composition, and HTTP composition. Do not let a single host module accumulate all transport, runtime, and application wiring.
- HTTP tenant resolution sources remain Host-side extensions. Register them through a dedicated Host builder/module, not by pushing HTTP concerns down into `ChengYuan.MultiTenancy.Runtime`.
- CLI hosts do not participate in the current multi-tenancy design. The CLI host runs without tenant resolution and does not expose CLI-specific tenant input adapters.

## Core Foundation Design

The foundational split is clear: `ChengYuan.Core` owns foundational platform concerns, and the old `ChengYuan.Domain` baseline is no longer part of the active architecture.

The rule is simple:

- `Core` owns foundational platform concepts.
- provider concerns stay in adjacent framework modules.
- technical systems such as `ExecutionContext`, `MultiTenancy`, `Caching`, and `Outbox` build on top of `Core` instead of being folded into it.
- the retired `ChengYuan.Domain` baseline must not be reintroduced.

### Target Core Family

| Module | Owns | Must Not Own | Depends On |
|---|---|---|---|
| `ChengYuan.Core` | Base exceptions, error codes, `Result`, DDD primitives, strongly typed IDs, object extension contracts, `IClock`, `IGuidGenerator` | Json, EF Core, tenant context, current user context, caching, outbox | Nothing internal |
| `ChengYuan.Core.Runtime` | Module descriptors, module catalog, lifecycle hooks, module bootstrap and ordering, `ServiceConfigurationContext`, init logging (`IInitLoggerFactory`, `IInitLogger<T>`), logging utilities (`IHasLogLevel`, `IExceptionWithSelfLogging`) | Domain primitives, serializer providers, data providers | `ChengYuan.Core` |
| `ChengYuan.Core.Json` | Serializer abstractions, System.Text.Json integration, strongly typed ID converters | EF Core, repository or UoW logic | `ChengYuan.Core` |
| `ChengYuan.Core.Validation` | Validation contracts and default validation pipeline | Localization resources, persistence providers | `ChengYuan.Core` |
| `ChengYuan.Core.Localization` | Resource registration and exception or error localization seams | Validation runtime policy, persistence providers | `ChengYuan.Core` |
| `ChengYuan.Core.Data` | Repository contracts, unit of work contracts, data filters, persistence-facing traits such as soft delete | EF Core-specific implementation details | `ChengYuan.Core` |
| `ChengYuan.Core.EntityFrameworkCore` | EF Core model conventions, value converters, repository and UoW provider implementations | Core abstractions or non-EF provider rules | `ChengYuan.Core`, `ChengYuan.Core.Data` |

### Core Public Surface Priorities

1. Modularity first: `DependsOnAttribute`, `ModuleBase`, module descriptors, module catalog, ordering logic, and lifecycle hooks such as pre-configure and post-configure.
2. Failure model second: `ChengYuanException`, `BusinessException`, stable `ErrorCode` support, `Result`, and `Result<T>` built on one shared error shape.
3. DDD primitives third: `Entity`, `AggregateRoot`, `ValueObject`, domain event collection, strongly typed IDs, `IClock`, and `IGuidGenerator`.
4. Support modules next: Json, Validation, and Localization become explicit extension seams instead of being scattered into runtime or host projects.
5. Data abstractions after that: repositories, unit of work, and data filters such as soft delete and multi-tenancy integration points.
6. Provider implementation last: EF Core conventions, strongly typed ID converters, repositories, filters, and persistence helpers.

### Current-To-Target Migration Map

| Current Location | Target Location | Migration Rule |
|---|---|---|
| `ChengYuan.Hosting` module primitives | `ChengYuan.Core.Runtime` | `Hosting` stops owning modularity and becomes a thin composition helper only |
| `ChengYuan.Domain` result and strongly typed ID primitives | `ChengYuan.Core` | Core primitives move under explicit namespaces and stop pretending to be a separate domain module |
| `ChengYuan.Domain` Json converters | `ChengYuan.Core.Json` | Serializer support becomes a provider-adjacent module, not part of the core runtime |
| `ChengYuan.Domain.EntityFrameworkCore` converters | `ChengYuan.Core.EntityFrameworkCore` | EF Core support depends on Core; Core never depends on EF Core |
| `ExecutionContext.IClock` | `ChengYuan.Core` | Move the abstraction into Core, keep runtime implementation migration-friendly |
| `MultiTenancy` tenant contracts | stay in `ChengYuan.MultiTenancy` | Integrate later through `Core.Data` filters rather than moving tenant context into Core |

### Implementation Waves

1. Wave A: create the `ChengYuan.Core` family and move modularity ownership out of `ChengYuan.Hosting`.
2. Wave B: establish the core failure model and DDD baseline with exceptions, error codes, `Result`, entities, aggregate roots, value objects, strongly typed IDs, `IClock`, and `IGuidGenerator`.
3. Wave C: add `ChengYuan.Core.Json`, `ChengYuan.Core.Validation`, `ChengYuan.Core.Localization`, and `ChengYuan.Core.Data`, then enforce their dependency boundaries with architecture tests.
4. Wave D: add `ChengYuan.Core.EntityFrameworkCore` for converters, repositories, unit of work, and data-filter implementations.
5. Wave E: rebase `ExecutionContext`, `MultiTenancy`, `Caching`, `Outbox`, and later `Authorization`, `Settings`, `Features`, and `Auditing` on the new Core family.
6. Wave F: split and tighten tests into core modularity, core primitives, provider, and framework-kernel suites.
7. Wave G: retire the old `ChengYuan.Domain` naming after all references have moved. Completed.

### Architecture Guardrails

- `Core` must not depend on `ExecutionContext`, `MultiTenancy`, `Caching`, `Outbox`, Json runtime, or EF Core.
- `Hosting` must not own module base types, module descriptors, or lifecycle contracts.
- Json and EF Core support must remain opt-in module dependencies, not behavior hidden inside `Core` itself.
- Only aggregate roots should get repository defaults in the data layer.
- `CurrentUser` and `CurrentTenant` stay in their own technical systems and do not move into `Core`.
- Do not pull wide package sprawl, dynamic proxy infrastructure, auto-generated API layers, or UI framework layers into the first Core refactor.

## Phase 1 Module Catalog

### Framework Family

- `ChengYuan.Core`
- `ChengYuan.Core.Runtime`
- `ChengYuan.Core.Json`
- `ChengYuan.Core.Validation`
- `ChengYuan.Core.Localization`
- `ChengYuan.Core.Data`
- `ChengYuan.Core.EntityFrameworkCore`
- `ChengYuan.Hosting`
- `ChengYuan.ExecutionContext`
- `ChengYuan.MultiTenancy`
- `ChengYuan.Authorization`
- `ChengYuan.Settings`
- `ChengYuan.Auditing`
- `ChengYuan.Caching`
- `ChengYuan.Outbox`

### Application Family

- `ChengYuan.Identity`
- `ChengYuan.PermissionManagement`
- `ChengYuan.TenantManagement`
- `ChengYuan.SettingManagement`
- `ChengYuan.AuditLogging`
- `ChengYuan.FeatureManagement` as a later phase once the first skeleton is stable

User business modules created later from templates also belong in `src/Applications`.

## Dependency Rules

- `Framework` modules may depend only on other `Framework` modules and approved external libraries.
- All `Framework` and `Application` modules may depend on `ChengYuan.Core`.
- Only hosts and framework runtime facets that participate in module bootstrap may depend on `ChengYuan.Core.Runtime`.
- `Application` modules may depend on `Framework` modules and on other `Application` modules through `Contracts` only.
- No module may depend on another module's `Persistence`, `Web`, or `Cli` facet.
- `Web` and `Cli` facets should resolve application services through DI and depend on contracts, not on implementation projects.
- Hosts may depend on framework runtime facets and selected application transport facets only.
- Cross-module reads should prefer lookup or integration service contracts.
- Cross-module side effects should prefer local events in the modular monolith and remain compatible with distributed events later.
- Provider and contributor patterns belong in the owning framework or application modules, not in hosts.
- `Hosting` must not be used as a backdoor dependency for modularity primitives after the Core refactor begins.

## Host Composition

### Web Host

`ChengYuan.CliHost` is a thin command-oriented shell. It composes:

- selected framework runtime facets
- selected application modules or future `Cli` facets
- CLI-specific runtime glue and scripting-friendly output

Internally, `CliHost` should follow the same host layering rule as `WebHost`:

- `CliHostFrameworkCompositionModule` for framework runtime dependencies
- `CliHostApplicationCompositionModule` for selected application modules
- `CliHostRuntimeGlueModule` for CLI-only runtime glue

`Program.cs` should stay minimal and delegate to host composition seams such as `AddCliHostComposition(...)` and `RunCliHostCompositionAsync()`.

CLI scenarios may intentionally omit modules that are irrelevant to command execution. Web transport facets should not be pulled into `CliHost`, but persistence-backed application modules are acceptable when the command workload needs them.

### CLI Host

`ChengYuan.CliHost` is a command-oriented shell built on System.CommandLine and Spectre.Console. It composes:

- selected framework runtime facets
- selected application `Cli` facets
- console UX and scripting-friendly output

It may intentionally omit modules that are irrelevant to a command-line scenario. Authentication-oriented modules, UI-heavy modules, or some persistence-heavy modules are optional, provided declared dependencies remain satisfied.

## Rules For Future Development

1. Choose the module family before creating any project.
2. Choose the smallest facet set that satisfies the use case.
3. Do not create empty `Web`, `Cli`, or `Persistence` projects just for symmetry.
4. Keep the directory shape `family root -> short module folder -> project`.
5. Do not put family words in project names, but keep facet words in project names when they describe the project role.
6. Add architecture tests whenever a new module or facet is introduced.
7. Promote recurring technical concerns into `Framework`, not into shared application dumping grounds.
8. Promote reusable management capabilities into `Applications`, not into hosts.
9. Keep hosts thin and declarative.

## Practical Defaults

- Start phase 1 with one shallow framework module such as `Caching`, one stateful framework module such as `Outbox`, and one full application module such as `Identity`.
- Before expanding more framework or management modules, keep the `ChengYuan.Core` ownership split intact and do not reintroduce the retired `ChengYuan.Domain` baseline.
- Use those three shapes to validate that variable-depth modules remain coherent.
- Once those shapes are stable, expand the rest of the framework and application families.

## Related Pages

- [Introduction](./introduction.md)
- [Development](../contributing/development.md)