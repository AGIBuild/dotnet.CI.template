# dotnet.CI.template Documentation

An out-of-the-box .NET CI/CD template that standardizes build, test, publish, and documentation deployment.

## Getting Started

- [Quick Start: Release in 2 Minutes](quick-start-release.md)
- [GitHub Workflows Guide](github-workflows-guide.md)

## Documentation Hosting

- Expected URL: `https://agibuild.github.io/dotnet.CI.template/`
- The workflow builds VitePress and deploys to GitHub Pages in the `deploy-docs` job
- If `docs/package.json` is missing or Pages is not enabled, documentation deployment is skipped
