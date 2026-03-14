# CI/CD Pipeline

The goal of this pipeline is straightforward:
**Every commit gets quality feedback, releases are automated, artifacts are traceable and reproducible.**

```text
push / PR to main
  └─ CI and Release
       ├─ resolve-version
       ├─ build-and-test (matrix: all platforms; release adds publish)
       ├─ build-docs (VitePress build, pre-check on every push/PR)
       ├─ release (requires approval, NuGet push + tag + GitHub Release)
       └─ deploy-docs (deploys pre-built docs to GitHub Pages)

push / PR to main + weekly
  └─ CodeQL (security scan)
```

For a quick release walkthrough, see: [Releasing](releasing.md).

---

## 1) The Two Workflows

### `CI and Release`
- Triggers: `push` to `main`, `pull_request` targeting `main`, manual `workflow_dispatch`
- Concurrency: Grouped by branch (e.g. `ci-refs/heads/main`), running workflows complete normally; only pending (queued) workflows are replaced by newer pushes
- PR behavior: Runs multi-platform Build + Test (Format Check + Coverage Report on linux only, Pack verification on linux) with prerelease suffix
- main push behavior: If `VersionPrefix` has been bumped (tag doesn't exist yet), triggers full-platform matrix Build + Test + Pack + Publish + PackageApp → approval → NuGet push + SBOM + Attestation + tag + GitHub Release → documentation deployment. Otherwise, runs CI-only (multi-platform build + test + linux pack verification).
- Artifacts: Test results (with PR annotations), coverage reports, NuGet packages (with release manifest), platform installer zips, SBOM

### `CodeQL`
- Triggers: `push`/`pull_request` to `main`, plus weekly scheduled scan
- Purpose: Security analysis (C#)
- Note: Uses `./build.sh Build` for a unified build entry point

---

## 2) Job Details

### `resolve-version`
- Reads `VersionPrefix` from `Directory.Build.props`, validates semver format (3-segment: `Major.Minor.Patch`)
- Determines if this is a release: main push + tag `v{version}` does not exist yet = release; otherwise CI-only
- Computes the build matrix: all modes use multi-platform (linux/win/osx); release additionally enables publish

### `build-and-test`
- Matrix build: each platform runs Build + Test
- Linux runner additionally runs Format Check (`dotnet format --verify-no-changes`) and Coverage Report
- PR: uses `--VersionSuffix "ci.<run_number>"`
- main push: no suffix (locks the release version)
- Linux runner additionally runs Pack (always, as a pre-release quality gate); GenerateReleaseManifest runs only for releases
- For releases, all platforms with publish enabled run Publish + PackageApp, producing installer zips (`app-{runtime}.zip`)
- Test results are displayed directly in PR checks via `dorny/test-reporter`

### `release`
- Requires `release` environment approval (single approval gate)
- If `ENABLE_NUGET` is `true`: downloads NuGet packages, verifies release manifest SHA256 integrity, pushes to nuget.org via API key or OIDC (controlled by `NUGET_USE_OIDC`)
- If `ENABLE_INSTALLERS` is `true`: downloads platform installer zips
- Generates SBOM (SPDX format, `anchore/sbom-action`)
- Creates Artifact Attestation for all available artifacts (NuGet packages and/or installer zips)
- Creates a git tag and pushes to remote
- Creates a GitHub Release with available assets (installer zips and/or SBOM)

### `build-docs`
- Runs on every push/PR, in parallel with `build-and-test` (pre-check)
- If `docs/package.json` exists, installs dependencies and builds VitePress
- Uploads the built site as an artifact (`docs-site`) for `deploy-docs` to consume
- Catches documentation build errors early — before release

### `deploy-docs`
- Runs automatically after a successful `release`, downloads the pre-built docs artifact from `build-docs`
- If GitHub Pages is not enabled, it skips gracefully with a notice — it will not fail the pipeline

---

## 3) Environment Configuration

### `release` environment
In GitHub repository Settings → Environments → create `release`:
- **Required reviewers**: Add at least one reviewer
- Optional: Configure wait timer, deployment branches (restrict to `main`)

### `github-pages` environment (Optional)
The repository includes built-in VitePress documentation support. To enable documentation publishing:
- Settings → Pages → Source: select GitHub Actions
- The `github-pages` environment is created automatically
- Expected documentation URL: `https://<owner>.github.io/<repo>/`
- The `Resolve Version` summary displays this URL for quick access

### Feature Switches

All switches live in `ci.yml` under the top-level `env:` block. Change their values to `'true'` or `'false'` to enable or disable pipeline features.

| Switch | Default | Purpose |
|--------|---------|---------|
| `ENABLE_NUGET` | `true` | NuGet package generation (Pack) and publishing to nuget.org |
| `NUGET_USE_OIDC` | `false` | `false` = push via `NUGET_API_KEY` secret; `true` = push via OIDC Trusted Publishing |
| `ENABLE_INSTALLERS` | `true` | Platform installer zips (Publish + PackageApp) |
| `ENABLE_ANDROID` | `false` | Include Android workload in the build matrix |
| `ENABLE_IOS` | `false` | Include iOS workload in the build matrix |

When `ENABLE_NUGET` is `true`:
- If `NUGET_USE_OIDC` is `false`, the `NUGET_API_KEY` secret must be configured; otherwise the release fails with an explicit error.
- If `NUGET_USE_OIDC` is `true`, a Trusted Publishing trust policy must be configured on nuget.org for the repository.

### Secrets
- `NUGET_API_KEY`: NuGet.org API key (required when `ENABLE_NUGET` is `true` and `NUGET_USE_OIDC` is `false`)

---

## 4) Version Mechanism FAQ

### Q1: Where does the version come from?
The version comes from `VersionPrefix` in `Directory.Build.props`. CI does not accept manually input version parameters.

### Q2: How to upgrade the version?

```bash
./build.sh ShowVersion                           # Show current version
./build.sh UpdateVersion                         # Patch increment: 0.2.0 -> 0.2.1
./build.sh UpdateVersion --VersionPrefix 1.0.0   # Set explicitly
```

After making changes, merge to `main` via PR. CI automatically builds with the new version.

### Q3: Can the same version be re-published?
No. Each version can only be released once. If the tag `v{version}` already exists, the release is skipped and the run becomes CI-only. To publish a new version, bump `VersionPrefix` via PR (e.g., `0.2.0` → `0.3.0`).

### Q4: What is the release manifest?
`release-manifest.json` records the SHA256 hash and version information for each NuGet package. During the release phase, package files are verified against the manifest to prevent tampering or corruption during artifact transfer.

---

## 5) CLI Trigger (Optional)

```bash
# Manually trigger CI and Release
gh workflow run ci.yml --ref main
```

---

## 6) Troubleshooting

- Tag already exists: This version was already published; bump `VersionPrefix` via PR
- NuGet push failed: Verify that `NUGET_API_KEY` is configured
- Package version validation failed: Check that `VersionPrefix` in `Directory.Build.props` is correct
- Hash mismatch: Artifacts were corrupted during job-to-job transfer; re-trigger the workflow
- Windows/Linux inconsistency: Ensure `.gitattributes` is in effect, especially `*.sh` with LF
- Documentation deployment skipped: Expected behavior; configure `docs/package.json` first

---

## 7) Team Collaboration Tips

- During daily development, just watch for green CI (Build + Test + CodeQL)
- Release management is controlled through environment approval — no manual packaging or artifact transfer
- When changing workflow logic, extend NUKE targets first, then update workflow orchestration

This gives your team a stable experience:
**Fast development, fast feedback, reliable releases.**
