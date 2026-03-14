# API Reference

## Dotnet.CI.Template.Sample

### `Calculator`

A simple arithmetic calculator.

#### Methods

| Method | Signature | Description |
|---|---|---|
| `Add` | `int Add(int a, int b)` | Returns the sum of two integers. |
| `Divide` | `int Divide(int a, int b)` | Returns the integer quotient. Throws `DivideByZeroException` when `b` is zero. |

#### Example

```csharp
var calc = new Calculator();
calc.Add(1, 2);      // 3
calc.Divide(10, 3);  // 3
```

---

> **Tip for template users**: Replace this page with your own API documentation. Consider using [DocFX](https://dotnet.github.io/docfx/) or [xmldoc2md](https://github.com/nicolestandifer3/xmldoc2md-csharp) for auto-generated API docs from XML comments.
