# GitHub Workflows Guide

The goal of these workflows is straightforward:
**Every commit gets quality feedback, releases are automated, artifacts are traceable and reproducible.**

If you just created a repository from this template, think of it as a standard pipeline:

```text
push / PR to main
  └─ CI and Release
       ├─ resolve-version
       ├─ build-and-test (matrix: PR=linux, main=all platforms)
       ├─ release (requires approval, NuGet push + tag + GitHub Release)
       └─ deploy-docs (GitHub Pages)

push / PR to main + weekly
  └─ CodeQL (security scan)
```

For a quick release walkthrough, see: [Quick Start Release](quick-start-release.md).

---

## 1) The Two Workflows

### `CI and Release`
- Triggers: `push` to `main`, `pull_request` targeting `main`, manual `workflow_dispatch`
- Concurrency: **Parallel by default** — multiple pushes can run simultaneously (`run_number` is naturally unique, no version conflicts)
  - For serial execution: Settings → Variables → set `CI_SERIAL=true` (queues by branch)
  - To cancel older runs: set `CI_CANCEL_IN_PROGRESS=true`
- PR behavior: Runs Format Check + Build + Test + Coverage Report on ubuntu (with prerelease suffix)
- main push behavior: Full-platform matrix Build + Test + Pack + Publish + PackageApp → approval → NuGet push + SBOM + Attestation + tag + GitHub Release → documentation deployment
- Artifacts: Test results (with PR annotations), coverage reports, NuGet packages (with release manifest), platform installer zips, SBOM

### `CodeQL`
- Triggers: `push`/`pull_request` to `main`, plus weekly scheduled scan
- Purpose: Security analysis (C#)
- Note: Uses `./build.sh Build` for a unified build entry point

---

## 2) Job Details

### `resolve-version`
- Reads `VersionPrefix` from `Directory.Build.props`, validates semver format
- Determines if this is a release (main push = true, PR = false)
- Computes the build matrix: PR uses ubuntu only, main push includes win/linux/osx

### `build-and-test`
- Matrix build: each platform runs Build + Test
- Linux runner additionally runs Format Check (`dotnet format --verify-no-changes`) and Coverage Report
- PR: uses `--VersionSuffix "ci.<run_number>"`
- main push: no suffix (locks the release version)
- Linux runner additionally runs Pack + GenerateReleaseManifest (generates SHA256 manifest)
- All platforms run Publish + PackageApp, producing installer zips (`app-{runtime}.zip`)
- Test results are displayed directly in PR checks via `dorny/test-reporter`

### `release`
- Requires `release` environment approval (single approval gate)
- Downloads NuGet packages, verifies release manifest SHA256 integrity
- Pushes NuGet packages to nuget.org (skipped if `NUGET_API_KEY` is not configured)
- Generates SBOM (SPDX format, `anchore/sbom-action`)
- Creates Artifact Attestation (`actions/attest-build-provenance`, signs build provenance for NuGet packages and installer zips)
- Creates a git tag and pushes to remote
- Creates a GitHub Release with platform installer zips + SBOM file attached

### `deploy-docs`
- Runs automatically after a successful `release`
- If `docs/package.json` exists, builds VitePress and deploys to GitHub Pages
- If GitHub Pages is not enabled or documentation config is missing, it skips gracefully with a notice — it will not fail the pipeline

---

## 3) Quickest Path to Get Started (Recommended)

1. Push a commit to `main`
   Verify that both `CI and Release` and `CodeQL` are green.

2. Go to Actions → find the workflow run → click **Review deployments**
   Approve the `release` environment.

3. Check the Releases page:
   - A tag was created (e.g., `v0.2.0.42`)
   - The GitHub Release contains platform installer zips
   - A corresponding package version exists on NuGet.org (if `NUGET_API_KEY` is configured)

---

## 4) Environment Configuration (Required)

### `release` environment
In GitHub repository Settings → Environments → create `release`:
- **Required reviewers**: Add at least one reviewer
- Optional: Configure wait timer, deployment branches (restrict to `main`)

### `github-pages` environment (Optional)
The repository includes built-in VitePress documentation support. To enable documentation publishing:
- Settings → Pages → Source: select GitHub Actions
- The `github-pages` environment is created automatically
- Expected documentation URL: `https://<owner>.github.io/<repo>/` (for this repo: `https://agibuild.github.io/dotnet.CI.template/`)
- The `Resolve Version` summary displays this URL for quick access

### Secrets
- `NUGET_API_KEY`: NuGet.org API key (configure at the repo or `release` environment level)

---

## 5) Version Mechanism FAQ (Important)

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
Each main push generates a unique 4-segment version number (e.g., `0.2.0.42`), so the same push won't conflict. To publish a new version (e.g., `0.3.0`), modify `VersionPrefix` via PR.

### Q4: What is the release manifest?
`release-manifest.json` records the SHA256 hash and version information for each NuGet package. During the release phase, package files are verified against the manifest to prevent tampering or corruption during artifact transfer.

---

## 6) CLI Trigger (Optional)

```bash
# Manually trigger CI and Release
gh workflow run ci.yml --ref main
```

---

## 7) Common Troubleshooting (Check Here First)

- Tag already exists: This version was already published; bump `VersionPrefix` via PR
- NuGet push failed: Verify that `NUGET_API_KEY` is configured
- Package version validation failed: Check that `VersionPrefix` in `Directory.Build.props` is correct
- Hash mismatch: Artifacts were corrupted during job-to-job transfer; re-trigger the workflow
- Windows/Linux inconsistency: Ensure `.gitattributes` is in effect, especially `*.sh` with LF
- Documentation deployment skipped: Expected behavior; configure `docs/package.json` first

---

## 8) Team Collaboration Tips

- During daily development, just watch for green CI (Build + Test + CodeQL)
- Release management is controlled through environment approval — no manual packaging or artifact transfer
- When changing workflow logic, extend NUKE targets first, then update workflow orchestration

This gives your team a stable experience:
**Fast development, fast feedback, reliable releases.**
