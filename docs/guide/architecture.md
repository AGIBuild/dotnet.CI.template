# Architecture

This page is the authoritative architecture guide for ChengYuan.

When existing code and this guide differ, use this guide as the default rule for new work and move the codebase toward these rules instead of extending the divergence.

## Design Principles

- Module first, layer second.
- Split reusable slices into `Framework` modules and `Application` modules so technical systems stay separate from reusable application capabilities.
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
│   └── ChengYuan.Caching/
│       ├── ChengYuan.Caching.Abstractions/
│       ├── ChengYuan.Caching.Runtime/
│       └── ChengYuan.Caching.Redis/
├── Applications/
│   └── ChengYuan.Identity/
│       ├── ChengYuan.Identity.Contracts/
│       ├── ChengYuan.Identity.Domain/
│       ├── ChengYuan.Identity.Application/
│       ├── ChengYuan.Identity.Persistence/
│       ├── ChengYuan.Identity.Web/
│       └── ChengYuan.Identity.Cli/
└── Hosts/
    ├── ChengYuan.WebHost/
    └── ChengYuan.CliHost/
```

Topology terms belong in directories, not in project names. Prefer `ChengYuan.Identity.Application` over `ChengYuan.Applications.Identity.Application`.

## Module Families

| Family | Purpose | Typical Examples | Notes |
|---|---|---|---|
| `Framework` | Technical systems, abstractions, runtime services, provider models, infrastructure bridges | `Core`, `ExecutionContext`, `Caching`, `Authorization`, `Settings`, `Outbox`, `Hosting` | Should not own user-facing business capabilities |
| `Applications` | Reusable application capabilities and business bounded contexts | `Identity`, `PermissionManagement`, `TenantManagement`, `SettingManagement`, later user business modules | Can own domain behavior, management use cases, persistence, and optional transports |
| `Hosts` | Runnable composition shells | `WebHost`, `CliHost` | Compose modules, policies, middleware, and transport glue only |

`ChengYuan.Core` is the only framework module family member allowed to own foundational modularity, failure, DDD, and shared extension seams. `ChengYuan.Hosting` stays as a thin composition helper and must not own the module model.

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
| `ChengYuan.Core.Runtime` | Module descriptors, module catalog, lifecycle hooks, module bootstrap and ordering | Domain primitives, serializer providers, data providers | `ChengYuan.Core` |
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

`ChengYuan.WebHost` is an API-first ASP.NET Core shell. It composes:

- selected framework runtime facets
- selected application `Web` facets
- middleware, auth integration, health checks, and transport policies

It does not own application use cases.

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
4. Keep the directory shape `family -> module -> project`.
5. Keep topology names out of project names.
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