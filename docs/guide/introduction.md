# Introduction

Dotnet.CI.Template is a GitHub template repository that gives new .NET projects a production-grade starting point:

- **NUKE build system** — typed C# build targets replace fragile shell scripts.
- **GitHub Actions CI/CD** — multi-platform build, test, pack, publish, and release in a single workflow.
- **Version management** — `VersionPrefix` in `Directory.Build.props` is the single source of truth; CI decides whether to release based on git tags.
- **Documentation site** — VitePress with i18n (English + Chinese), built and deployed to GitHub Pages automatically.

## What You Get

| Artifact | Description |
|---|---|
| NuGet package | Library packaged with symbols and release manifest |
| Platform installers | Self-contained app archives (`app-{runtime}.zip`) |
| SBOM | SPDX JSON for supply-chain transparency |
| Documentation | Static site deployed to GitHub Pages |

## Project Layout

```text
├── src/                    # Source projects
├── tests/                  # Test projects
├── build/                  # NUKE build targets
├── docs/                   # VitePress documentation
├── .github/workflows/      # CI/CD pipelines
├── Directory.Build.props   # Shared build properties & version
└── Dotnet.CI.Template.slnx # Solution file
```

## Next Steps

- [Getting Started](getting-started.md) — install and use the library.
- [Development](../contributing/development.md) — set up a local dev environment.
