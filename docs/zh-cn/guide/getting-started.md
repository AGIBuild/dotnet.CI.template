# 快速开始

## 校验当前骨架

先还原、构建并测试解决方案：

```bash
dotnet restore ChengYuan.slnx
dotnet build ChengYuan.slnx
dotnet test --solution ChengYuan.slnx
```

## 运行 Host

启动两个轻量组合壳，确认当前模块图可正常装配：

```bash
dotnet run --project src/Hosts/ChengYuan.WebHost
dotnet run --project src/Hosts/ChengYuan.CliHost
```

Web Host 会暴露 `/health`，CLI Host 会输出已加载模块和相关性信息。

## 扩展模板

1. 在 [GitHub 仓库](https://github.com/AGIBuild/dotnet.CI.template)点击 **Use this template**。
2. 在模块家族稳定前，保持解决方案和项目名沿用 `ChengYuan.*` 基线。
3. 按架构规则在 `src/Framework` 或 `src/Applications` 下新增模块。
4. 仅在确实需要对应传输方式时，把模块接入 `ChengYuan.WebHost` 或 `ChengYuan.CliHost`。
5. 更新 `Directory.Build.props` 中的包元数据。
6. 在 GitHub Settings 中配置 `release` environment（参见[发版指南](../contributing/releasing.md)）。

## 下一步

- [API 参考](../reference/api.md)
- [开发环境](../contributing/development.md)
