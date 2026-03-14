# Getting Started

## Installation

Add the NuGet package to your project:

```bash
dotnet add package Dotnet.CI.Template.Sample
```

## Usage

```csharp
using Dotnet.CI.Template.Sample;

var calculator = new Calculator();

var sum = calculator.Add(2, 3);       // 5
var quotient = calculator.Divide(10, 2); // 5
```

## Using as a Template

1. Click **Use this template** on the [GitHub repository](https://github.com/AGIBuild/dotnet.CI.template).
2. Run the init wizard to rename solution, projects, and namespaces:

```bash
./init.sh              # Linux / macOS
./init.ps1             # Windows (PowerShell)
```

3. Replace the sample `Calculator` class with your own code.
4. Update `Directory.Build.props` with your package metadata.
5. Configure the `release` environment in GitHub Settings (see [Releasing](../contributing/releasing.md)).

## Next Steps

- [API Reference](../reference/api.md)
- [Development Setup](../contributing/development.md)
