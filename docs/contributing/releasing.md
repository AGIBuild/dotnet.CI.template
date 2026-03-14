# Releasing

Audience: Team members shipping a new version.
Goal: **Complete a standard Release quickly**.

---

## 0) Prerequisites

- You have write access to the repository
- `main` branch is up to date and CI is passing
- If `ENABLE_NUGET` is `true`: the `NUGET_API_KEY` secret (or OIDC trust policy) is configured
- A `release` environment is created in GitHub (Settings → Environments) with required reviewers

---

## 1) Release Flow Overview

Release is integrated into the `CI and Release` workflow, consisting of 5 jobs:

```text
                ┌─ build-and-test (matrix) → release (requires approval) ─┐
resolve-version─┤                                                         ├→ deploy-docs
                └─ build-docs (pre-check) ────────────────────────────────┘
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

## 3) Success Criteria

After a successful run, you should see:

- A new tag (e.g., `v0.2.0`)
- A new GitHub Release
- Release assets: installer zips (if `ENABLE_INSTALLERS` is `true`) and the SBOM file
- A corresponding package version on NuGet.org (if `ENABLE_NUGET` is `true`)
- If Pages is enabled: documentation deployed automatically

---

## 4) Version Management

The version comes from `VersionPrefix` in `Directory.Build.props` — a pure 3-segment SemVer (e.g., `0.2.0`). A release is triggered only when this version differs from the latest git tag.

```bash
./build.sh ShowVersion                           # Show current version
./build.sh UpdateVersion                         # Patch increment: 0.2.0 -> 0.2.1
./build.sh UpdateVersion --VersionPrefix 1.0.0   # Set explicitly
```

After committing the change to `main`, CI automatically builds and publishes with the new version.

---

## 5) Troubleshooting

- `Tag already exists`: This version was already released; bump `VersionPrefix` and retry
- `No .nupkg files found`: No artifacts from the packaging stage; check Build/Test/Pack logs
- `Hash mismatch`: Build artifacts were corrupted during transfer; re-trigger the workflow
- NuGet push failed: Verify that `ENABLE_NUGET` is `true` and the credential (API key or OIDC) is configured correctly

---

## 6) CLI Trigger (Optional)

```bash
# Manually trigger the CI and Release workflow
gh workflow run ci.yml --ref main
```

---

For detailed pipeline information, see: [CI/CD Pipeline](ci-cd.md).
