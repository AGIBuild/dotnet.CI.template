# Development

## Prerequisites

| Tool | Version | Notes |
|---|---|---|
| .NET SDK | 10.0+ | Pinned in `global.json` (`rollForward: latestFeature`) |
| Git | 2.x | |
| Node.js | 18+ | Only needed for docs (`docs/package.json`) |

## Clone & Build

```bash
git clone https://github.com/AGIBuild/dotnet.CI.template.git
cd dotnet.CI.template

./build.sh          # Linux / macOS
build.ps1           # Windows
```

The default target is **Build** (Restore → Build).

## NUKE Build Targets

All build logic lives in `build/`. Workflows call these targets instead of raw `dotnet` commands.

| Target | Depends On | Description |
|---|---|---|
| **Restore** | — | `dotnet restore` + tool restore |
| **Build** | Restore | Build the solution |
| **Test** | Build | Run tests with coverage (trx + Coverlet) |
| **Pack** | Test | Pack NuGet packages (.nupkg + .snupkg) |
| **Publish** | Restore | Publish the app for a given `--Runtime` |
| **PackageApp** | Publish | Zip published output to `app-{runtime}.zip` |
| **Format** | Restore | `dotnet format --verify-no-changes` |
| **CoverageReport** | Test | Generate HTML + Cobertura coverage report |
| **ShowVersion** | — | Print current `VersionPrefix` |
| **UpdateVersion** | — | Bump patch or set `--VersionPrefix` explicitly |
| **GenerateReleaseManifest** | Pack | Create `release-manifest.json` with SHA256 hashes |

### Common Commands

```bash
./build.sh Test                                   # Build + Test
./build.sh Pack                                   # Build + Test + Pack
./build.sh Publish --Runtime linux-x64 --SelfContained  # Publish self-contained
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
| `--Runtime` | — | Target RID for `Publish` (e.g. `linux-x64`) |
| `--SelfContained` | `false` | Produce self-contained output |

### Artifacts

All outputs go to `artifacts/`:

```text
artifacts/
├── test-results/   # .trx files + coverage
├── packages/       # .nupkg + .snupkg + release-manifest.json
├── publish/        # dotnet publish output
└── installers/     # app-{runtime}.zip
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

- [CI/CD Pipeline](ci-cd.md) — how the GitHub Actions workflow operates.
- [Releasing](releasing.md) — how to ship a new version.
