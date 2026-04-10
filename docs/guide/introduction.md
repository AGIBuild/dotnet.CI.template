# Introduction

ChengYuan is the modular .NET template family maintained in this repository.

It is an opinionated .NET template family for modular monoliths, built around clear module boundaries, DDD+, and strict engineering guardrails. This documentation defines the default architecture and naming rules for ongoing development.

## Product Direction

ChengYuan is designed around five core ideas:

- **Module-first architecture** — every reusable capability starts as a vertical slice.
- **Two module families** — `Framework` modules for technical systems and `Application` modules for reusable capabilities and business contexts.
- **Composable hosts** — Web and CLI are thin hosts that load only the facets they need.
- **Variable module depth** — not every module needs Web, CLI, Persistence, or UI.
- **Strong delivery discipline** — build automation, lock files, versioning, tests, and docs remain mandatory.

## Target Topology

```text
src/
├── Framework/      # Technical modules and provider integrations
├── Applications/   # Reusable application modules and business modules
├── Hosts/          # WebHost and CliHost shells
├── ...
tests/              # Layer, integration, architecture, and template tests
build/              # NUKE build automation
docs/               # Documentation
```

Inside the source tree, the preferred shape is:

```text
family root -> short module folder -> project
```

For example, `src/Applications/Identity/ChengYuan.Identity.Application/` is preferred over `src/Applications/ChengYuan.Identity/ChengYuan.Identity.Application/`, and over introducing a role-only layer such as `src/Applications/Identity/Application/ChengYuan.Identity.Application/`.

Tests mirror the same organization. Keep module tests under `tests/ChengYuan.FrameworkKernel.Tests/<Family>/<Module>/...` and architecture suites under `tests/ChengYuan.ArchitectureTests/<Suite>/...`.

## What ChengYuan Is Not

- It is not a repository-wide horizontal clean architecture skeleton.
- It is not a general-purpose all-in-one application framework.
- It is not microservice-first.
- It does not assume that all modules must expose identical layers or transports.

## Read This Next

- [Architecture](./architecture.md) — the authoritative design rules for future development.
- [Getting Started](./getting-started.md) — how to use the current template assets.
- [Development](../contributing/development.md) — local workflow, build commands, and contributor rules.
