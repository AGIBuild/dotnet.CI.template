# API 参考

## Dotnet.CI.Template.Sample

### `Calculator`

简单的算术计算器。

#### 方法

| 方法 | 签名 | 说明 |
|---|---|---|
| `Add` | `int Add(int left, int right)` | 返回两个整数之和。 |
| `Divide` | `int Divide(int dividend, int divisor)` | 返回整数商。当 `divisor` 为零时抛出 `DivideByZeroException`。 |

#### 示例

```csharp
Calculator.Add(1, 2);      // 3
Calculator.Divide(10, 3);  // 3
```

---

> **模板使用提示**：请用你自己的 API 文档替换本页面。可以考虑使用 [DocFX](https://dotnet.github.io/docfx/) 或 [xmldoc2md](https://github.com/nicolestandifer3/xmldoc2md-csharp) 从 XML 注释自动生成 API 文档。
