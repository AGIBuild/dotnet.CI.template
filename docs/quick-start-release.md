# Quick Start: Release in 2 Minutes

Audience: New team members working with this repository for the first time.
Goal: **Complete a standard Release quickly**.

---

## 0) Prerequisites (30 seconds)

- You have write access to the repository
- `main` branch is up to date and CI is passing
- If publishing to NuGet, the `NUGET_API_KEY` secret is configured
- A `release` environment is created in GitHub (Settings → Environments) with required reviewers

---

## 1) Release Flow Overview

Release is integrated into the `CI and Release` workflow, consisting of 4 sequential jobs:

```text
resolve-version → build-and-test (matrix) → release (requires approval) → deploy-docs
```

After pushing to `main`:
1. CI automatically builds, tests, and packages (NuGet packages + platform installer zips)
2. The `release` job waits for **reviewer approval** on the `release` environment
3. After approval: NuGet push → SBOM generation + Artifact Attestation → tag + GitHub Release (installer zips + SBOM) → documentation deployment

---

## 2) How to Trigger a Release

**Automatic**: After merging a PR to `main`, CI runs automatically. Once the build completes, it enters the approval queue. Go to the Actions page, find the workflow run, and click **Review deployments** to approve the `release` environment.

**Manual**: On the Actions page, manually **Run workflow** (select the `main` branch).

---

## 3) Success Criteria (30-second check)

After a successful run, you should see:

- A new tag (e.g., `v0.2.0.42`)
- A new GitHub Release
- Release assets including platform installer zips (`app-linux-x64.zip`, `app-win-x64.zip`, etc.) and the SBOM file
- A corresponding package version on NuGet.org (if `NUGET_API_KEY` is configured)
- If Pages is enabled: documentation at `https://agibuild.github.io/dotnet.CI.template/`

---

## 4) Version Management (Must Read)

The version comes from `VersionPrefix` in `Directory.Build.props`. CI automatically appends the build number (e.g., `0.2.0.42`).

```bash
./build.sh ShowVersion                           # Show current version
./build.sh UpdateVersion                         # Patch increment: 0.2.0 -> 0.2.1
./build.sh UpdateVersion --VersionPrefix 1.0.0   # Set explicitly
```

After committing the change to `main`, CI automatically builds and publishes with the new version.

---

## 5) Common Failure Troubleshooting

- `Tag already exists`: This version was already released; bump `VersionPrefix` and retry
- `No .nupkg files found`: No artifacts from the packaging stage; check Build/Test/Pack logs
- `Hash mismatch`: Build artifacts were corrupted during transfer; re-trigger the workflow
- NuGet push failed: Verify that the `NUGET_API_KEY` secret is configured

---

## 6) CLI Trigger (Optional)

```bash
# Manually trigger the CI and Release workflow
gh workflow run ci.yml --ref main
```

---

For detailed information, see: [GitHub Workflows Guide](github-workflows-guide.md).
