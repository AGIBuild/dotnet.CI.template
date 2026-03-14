# Contributing

Thank you for your interest in contributing to this project!

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (version specified in `global.json`)
- [Node.js 22+](https://nodejs.org/) (only for documentation)

## Getting Started

```bash
git clone <repo-url>
cd dotnet.CI.template
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

## Development Workflow

1. Create a feature branch from `main`.
2. Make your changes, ensuring tests pass and code formatting is correct.
3. Submit a pull request targeting `main`.

## Code Style

- Code style is enforced by `.editorconfig` and `dotnet format`.
- Run `./build.sh Format` locally before committing.
- All warnings are treated as errors (`TreatWarningsAsErrors`).

## Versioning

- Version is managed via `VersionPrefix` in `Directory.Build.props`.
- A release is triggered automatically when a new version is pushed to `main` and the corresponding git tag does not yet exist.

## Public API Changes

- Packable projects track their public API surface via `PublicAPI.Shipped.txt` and `PublicAPI.Unshipped.txt`.
- When adding or modifying public APIs, update `PublicAPI.Unshipped.txt` accordingly.
