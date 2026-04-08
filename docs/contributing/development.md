# Development

## Architecture First

Before adding any project, package reference, or transport adapter, read [Architecture](../guide/architecture.md). That page is the authoritative design guidance for future development.

If the current source tree and the architecture guide differ, prefer the guide and move new work toward it.

## Core Development Rules

- Choose the module family first: `Framework` or `Applications`.
- Choose the smallest facet set that satisfies the use case. Do not create empty `Web`, `Cli`, or `Persistence` projects for symmetry.
- Keep topology terms in directories, not in project names.
- `Framework` modules may depend only on other `Framework` modules.
- `Application` modules may depend on `Framework` modules and on other `Application` modules through `Contracts` only.
- Hosts compose modules; hosts do not implement business use cases.
- Add architecture tests when you introduce a new module or a new facet profile.

## Prerequisites

| Tool | Version | Notes |
|---|---|---|
| .NET SDK | 10.0+ | Pinned in `global.json` (`rollForward: latestFeature`) |
| Git | 2.x | |
| Node.js | 18+ | Only needed for docs (`docs/package.json`) |

## Clone & Build

```bash
git clone <repo-url>
cd <repo-folder>

./build.sh          # Linux / macOS
build.ps1           # Windows
```

The default target is **Build** (Restore → Build).

## Module Authoring Checklist

1. Pick the module family.
2. Pick the module profile or facet set.
3. Add only the projects the module actually needs.
4. Declare direct dependencies explicitly.
5. Add or update architecture tests.
6. Wire the module into Web or CLI only if a transport facet is required.

## NUKE Build Targets

All build logic lives in `build/`. Workflows call these targets instead of raw `dotnet` commands.

| Target | Depends On | Description |
|---|---|---|
| **Restore** | — | `dotnet restore` + tool restore |
| **Build** | Restore | Build the solution |
| **Test** | Build | Run tests with coverage (trx + Coverlet) |
| **Pack** | Test | Pack NuGet packages (.nupkg + .snupkg) |
| **Publish** | Restore | Publish the selected host for a given `--Host` and `--Runtime` |
| **PackageApp** | Publish | Zip published output to `app-{host}-{runtime}.zip` |
| **Format** | Restore | `dotnet format --verify-no-changes` |
| **CoverageReport** | Test | Generate HTML + Cobertura coverage report |
| **ShowVersion** | — | Print current `VersionPrefix` |
| **UpdateVersion** | — | Bump patch or set `--VersionPrefix` explicitly |
| **GenerateReleaseManifest** | Pack | Create `release-manifest.json` with SHA256 hashes |

### Common Commands

```bash
./build.sh Test                                   # Build + Test
./build.sh Pack                                   # Build + Test + Pack
./build.sh Publish --Host web --Runtime linux-x64 --SelfContained  # Publish self-contained web host
./build.sh CoverageReport                         # Generate coverage HTML
./build.sh ShowVersion                            # Print version
./build.sh UpdateVersion                          # Patch bump (e.g. 0.2.0 → 0.2.1)
./build.sh UpdateVersion --VersionPrefix 1.0.0    # Set version explicitly
```

### Parameters

| Parameter | Default | Description |
|---|---|---|
| `--Configuration` | `Debug` (local) / `Release` (CI) | Build configuration |
| `--VersionPrefix` | — | Version to set (used by `UpdateVersion`) |
| `--VersionSuffix` | — | Prerelease suffix (e.g. `ci.42`) |
| `--Host` | — | Host to publish (`web` or `cli`) |
| `--Runtime` | — | Target RID for `Publish` (e.g. `linux-x64`) |
| `--SelfContained` | `false` | Produce self-contained output |

### Artifacts

All outputs go to `artifacts/`:

```text
artifacts/
├── test-results/   # .trx files + coverage
├── packages/       # .nupkg + .snupkg + release-manifest.json
├── publish/        # dotnet publish output grouped by host/runtime
└── installers/     # app-{host}-{runtime}.zip
```

## Docs Development

```bash
cd docs
npm install
npm run docs:dev      # Local dev server at http://localhost:5173
npm run docs:build    # Production build
npm run docs:preview  # Preview production build
```

## Next Steps

- [Architecture](../guide/architecture.md) — repository-wide design rules and module boundaries.
- [CI/CD Pipeline](ci-cd.md) — how the GitHub Actions workflow operates.
- [Releasing](releasing.md) — how to ship a new version.
