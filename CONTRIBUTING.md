# Contributing

Thank you for your interest in contributing to this project!

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (version specified in `global.json`)
- [Node.js 22+](https://nodejs.org/) (only for documentation)

## Getting Started

```bash
git clone <repo-url>
cd <repo-folder>
dotnet restore
dotnet build
dotnet test
```

## Build System

This project uses [NUKE](https://nuke.build/) for build automation. All build logic lives in `build/BuildTask.*.cs`.

Common targets:

| Target | Description |
|--------|-------------|
| `./build.sh Build` | Compile all projects |
| `./build.sh Test` | Run tests |
| `./build.sh CoverageReport` | Generate coverage report with threshold enforcement |
| `./build.sh Format` | Verify code formatting |
| `./build.sh Pack` | Create NuGet packages |
| `./build.sh ShowVersion` | Display current version |
| `./build.sh Benchmark` | Run BenchmarkDotNet benchmarks (requires `benchmarks/` projects) |
| `./build.sh MutationTest` | Run Stryker.NET mutation testing |

## CI Feature Switches

The CI pipeline behavior is controlled by boolean switches in `.github/workflows/ci.yml` under `env:`:

| Switch | Default | Purpose |
|--------|---------|---------|
| `ENABLE_NUGET` | `true` | NuGet package generation and publishing |
| `NUGET_USE_OIDC` | `false` | `false` = API key, `true` = Trusted Publishing (OIDC) |
| `ENABLE_INSTALLERS` | `true` | Platform installer zips (Publish + PackageApp) |
| `ENABLE_ANDROID` | `false` | Android workload in build matrix |
| `ENABLE_IOS` | `false` | iOS workload in build matrix |

## Development Workflow

1. Create a feature branch from `main`.
2. Make your changes, ensuring tests pass and code formatting is correct.
3. Submit a pull request targeting `main`.

## Code Style

- Code style is enforced by `.editorconfig` and `dotnet format`.
- Run `./build.sh Format` locally before committing.
- All warnings are treated as errors (`TreatWarningsAsErrors`).

## Module Taxonomy

New work should follow the logical taxonomy `FrameworkCore / Application / Extension / Host`:

- `FrameworkCore`: broadly reusable technical capabilities and modularity primitives.
- `Application`: reusable business capabilities and bounded contexts.
- `Extension`: optional modules that attach a capability to a concrete technology or transport, such as `Persistence`, `Web`, `Memory`, or future `Redis` and `MongoDb` adapters.
- `Host`: runnable shells that compose modules and provide environment configuration.

Use responsibility to classify a module, not directory name alone. A project under `src/Framework/` can still be an `Extension` if it is technology-specific, such as `ChengYuan.Caching.Memory`.

### Module Base Classes

New production modules must inherit a category-specific base class:

- `FrameworkCoreModule` for framework capabilities.
- `ApplicationModule` for business capabilities.
- `ExtensionModule` for persistence, web, memory, worker, or other technology adapters.
- `HostModule` for host shells and composition modules.

Direct `ModuleBase` inheritance is reserved for low-level modularity infrastructure only. The shared service-registration entry point `ConfigureServices(IServiceCollection services)` remains the same across all categories.

During the current architecture refactor:

- Keep explicit `DependsOn` module composition.
- Do not introduce automatic module discovery.
- Move application persistence registration out of Hosts and back into the corresponding `*.Persistence` extension modules.
- Keep Host internals layered. For WebHost, separate framework composition, application composition, and HTTP/runtime glue into different composition modules instead of one all-knowing host module.
- Keep `Program.cs` thin by routing setup through host composition seams such as `AddWebHostComposition(...)` and `UseWebHostComposition()`.

## Versioning

- Version is managed via `VersionPrefix` in `Directory.Build.props`.
- A release is triggered automatically when a new version is pushed to `main` and the corresponding git tag does not yet exist.

## Public API Changes

- Packable projects track their public API surface via `PublicAPI.Shipped.txt` and `PublicAPI.Unshipped.txt`.
- When adding or modifying public APIs, update `PublicAPI.Unshipped.txt` accordingly.
