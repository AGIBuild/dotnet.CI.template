# 快速开始

## 安装

将 NuGet 包添加到项目：

```bash
dotnet add package Dotnet.CI.Template.Sample
```

## 使用

```csharp
using Dotnet.CI.Template.Sample;

var sum = Calculator.Add(2, 3);       // 5
var quotient = Calculator.Divide(10, 2); // 5
```

## 作为模板使用

1. 在 [GitHub 仓库](https://github.com/AGIBuild/dotnet.CI.template)点击 **Use this template**。
2. 运行初始化向导，自动重命名解决方案、项目和命名空间：

```bash
./init.sh              # Linux / macOS
./init.ps1             # Windows (PowerShell)
```

3. 用你自己的代码替换示例 `Calculator` 类。
4. 更新 `Directory.Build.props` 中的包元数据。
5. 在 GitHub Settings 中配置 `release` environment（参见[发版指南](../contributing/releasing.md)）。

## 下一步

- [API 参考](../reference/api.md)
- [开发环境](../contributing/development.md)
