# dotnet.CI.template

[![CI and Release](https://github.com/AGIBuild/dotnet.CI.template/actions/workflows/ci.yml/badge.svg)](https://github.com/AGIBuild/dotnet.CI.template/actions/workflows/ci.yml)
[![CodeQL](https://github.com/AGIBuild/dotnet.CI.template/actions/workflows/codeql.yml/badge.svg)](https://github.com/AGIBuild/dotnet.CI.template/actions/workflows/codeql.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

**Stop wiring CI from scratch.** Start with a production-grade pipeline that builds, tests, packages, and releases your .NET projects across platforms -- with a single approval click.

---

## What You Get

Use this template and you instantly have:

```
Push to main
  |
  v
resolve-version ── Reads VersionPrefix, computes matrix, outputs version info
  |
  v
build-and-test ─── Matrix build (linux / windows / macos)
  |                 + Pack NuGet + Publish + Package installers
  |
  v
release ────────── [requires approval] NuGet push + Git tag + GitHub Release
  |
  v
deploy-docs ────── VitePress to GitHub Pages (optional, i18n)
```

On **pull requests**, only a fast linux build + test runs. On **main**, the full multi-platform pipeline kicks in.

### Pipeline Highlights

| Feature | Details |
|---------|---------|
| **NUKE Build** | All build logic lives in C# targets -- no scattered shell scripts in YAML |
| **Single Approval** | One `release` environment gate controls the entire release flow |
| **Multi-Platform Matrix** | linux-x64, win-x64, osx-arm64 (+ optional Android/iOS) |
| **NuGet Publishing** | Auto-push to NuGet.org with SHA256 manifest verification |
| **Installer Packages** | Platform-specific zip archives attached to GitHub Releases |
| **Version from Code** | `VersionPrefix` in `Directory.Build.props` is the single source of truth |
| **SemVer Release Tags** | `v0.2.0` format — release triggers only when version is bumped |
| **CodeQL Security** | Automated vulnerability scanning on every push and weekly |
| **Graceful Degradation** | Missing NuGet key? Skipped. No docs config? Skipped. Nothing breaks. |

---

## Quick Start

### 1. Create your repo from this template

Click **[Use this template](https://github.com/AGIBuild/dotnet.CI.template/generate)** on GitHub.

### 2. Configure environments

Go to **Settings > Environments** and create a `release` environment with at least one required reviewer.

### 3. Set secrets (optional)

| Secret | Purpose |
|--------|---------|
| `NUGET_API_KEY` | Push packages to NuGet.org |

### 4. Push code and watch it go

```bash
git push origin main
```

The pipeline runs automatically. When `build-and-test` completes, go to **Actions > Review deployments** to approve the release.

---

## Project Structure

```
.
├── .github/workflows/
│   ├── ci.yml              # CI + Release pipeline
│   └── codeql.yml          # Security analysis
├── build/
│   ├── BuildTask.*.cs      # NUKE targets (Build, Test, Pack, Publish, PackageApp, ...)
│   └── _build.csproj
├── src/                    # Your application code
├── tests/                  # Your test projects
├── docs/                   # VitePress documentation (English + 中文)
├── Directory.Build.props   # Version + shared build properties
└── global.json             # SDK version pinning
```

---

## Version Management

Versions follow SemVer (3-segment `Major.Minor.Patch`):

- **`VersionPrefix` in `Directory.Build.props`** is the single source of truth (e.g., `0.2.0`)
- **Tags are created automatically**: `v0.2.0` — release triggers only when the tag doesn't exist yet
- **`FileVersion`** includes the CI build number for traceability (e.g., `0.2.0.42`), visible in DLL properties
- No manual tagging. No version input fields. Bump `VersionPrefix` via PR to trigger a release.

### Version commands

```bash
./build.sh ShowVersion                           # show current version
./build.sh UpdateVersion                         # patch increment: 0.2.0 -> 0.2.1
./build.sh UpdateVersion --VersionPrefix 1.0.0   # set exact version
```

Then commit and push -- CI takes care of the rest.

---

## Extending the Build

Build logic is in `build/BuildTask.*.cs` using [NUKE](https://nuke.build). Add new targets in C# and call them from workflows:

```csharp
Target MyTarget => _ => _
    .DependsOn(Build)
    .Executes(() =>
    {
        // your build logic here
    });
```

```yaml
# in ci.yml
- run: ./build.sh MyTarget
```

---

## Mobile Platform Support

Android and iOS builds are supported but disabled by default. Toggle them in `ci.yml`:

```yaml
env:
  ENABLE_ANDROID: 'true'
  ENABLE_IOS: 'true'
```

---

## Concurrency

Multiple `CI and Release` runs execute **in parallel by default**.

To serialize runs on the same branch (only one active at a time), set repository variables in **Settings > Secrets and variables > Actions > Variables**:

| Variable | Default | Effect |
|----------|---------|--------|
| `CI_SERIAL` | _(unset / false)_ | Set to `true` to queue runs per branch instead of running in parallel |
| `CI_CANCEL_IN_PROGRESS` | _(unset / false)_ | Set to `true` to cancel the running workflow when a newer one is queued |

---

## Documentation Deployment

Documentation is built with [VitePress](https://vitepress.dev/) and supports English (default) and Chinese.

To enable automatic documentation deployment to GitHub Pages:

1. `docs/package.json` is included by default in this template
2. Enable GitHub Pages in **Settings > Pages > Source: GitHub Actions**
3. The `deploy-docs` job automatically builds and deploys after each release
4. Expected docs URL: `https://<owner>.github.io/<repo>/` (for this repo: `https://agibuild.github.io/dotnet.CI.template/`)

The `Resolve Version` job summary always shows this expected docs URL, even when docs deployment is skipped.
If Pages is not enabled, `deploy-docs` is skipped with a notice instead of failing the whole workflow.

---

## License

[MIT](LICENSE)
