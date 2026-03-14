# API Reference

## Dotnet.CI.Template.Sample

### `Calculator`

A simple arithmetic calculator.

#### Methods

| Method | Signature | Description |
|---|---|---|
| `Add` | `int Add(int left, int right)` | Returns the sum of two integers. |
| `Divide` | `int Divide(int dividend, int divisor)` | Returns the integer quotient. Throws `DivideByZeroException` when `divisor` is zero. |

#### Example

```csharp
Calculator.Add(1, 2);      // 3
Calculator.Divide(10, 3);  // 3
```

---

> **Tip for template users**: Replace this page with your own API documentation. Consider using [DocFX](https://dotnet.github.io/docfx/) or [xmldoc2md](https://github.com/nicolestandifer3/xmldoc2md-csharp) for auto-generated API docs from XML comments.
