# Getting Started

## Validate the Current Skeleton

Restore, build, and test the solution:

```bash
dotnet restore ChengYuan.slnx
dotnet build ChengYuan.slnx
dotnet test --solution ChengYuan.slnx
```

## Run the Hosts

Start the thin composition hosts to verify the current module graph:

```bash
dotnet run --project src/Hosts/ChengYuan.WebHost
dotnet run --project src/Hosts/ChengYuan.CliHost
```

The web host exposes `/health`, and the CLI host prints the loaded module list and correlation information.

## Extend the Template

1. Click **Use this template** on the [GitHub repository](https://github.com/AGIBuild/dotnet.CI.template).
2. Keep solution and project names aligned to the `ChengYuan.*` baseline until the module family stabilizes.
3. Add new modules under `src/Framework` or `src/Applications` based on the architecture rules.
4. Wire modules into `ChengYuan.WebHost` or `ChengYuan.CliHost` only when a transport is required.
5. Update `Directory.Build.props` with your package metadata.
6. Configure the `release` environment in GitHub Settings (see [Releasing](../contributing/releasing.md)).

## Next Steps

- [API Reference](../reference/api.md)
- [Development Setup](../contributing/development.md)
