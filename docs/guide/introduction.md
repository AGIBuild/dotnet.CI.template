# Introduction

ChengYuan is the target product direction of this repository.

It is an opinionated .NET template family for modular monoliths, built around ABP-style module boundaries, DDD+, and strict engineering guardrails. The repository is transitioning from its legacy CI template shape into this architecture model, so some implementation details may still reflect the earlier structure. For new work, treat the architecture described in this documentation as the target state.

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
family -> module -> project
```

For example, `src/Applications/ChengYuan.Identity/ChengYuan.Identity.Application/` is preferred over flattening all module projects into a single directory.

## What ChengYuan Is Not

- It is not a repository-wide horizontal clean architecture skeleton.
- It is not a full ABP runtime clone.
- It is not microservice-first.
- It does not assume that all modules must expose identical layers or transports.

## Read This Next

- [Architecture](./architecture.md) — the authoritative design rules for future development.
- [Getting Started](./getting-started.md) — how to use the current template assets.
- [Development](../contributing/development.md) — local workflow, build commands, and contributor rules.
