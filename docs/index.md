# dotnet.CI.template 文档首页

这是一个开箱即用的 .NET CI/CD 模板，目标是把构建、测试、发布和文档部署标准化。

## 你可以从这里开始

- [Quick Start: Release in 2 Minutes](quick-start-release.md)
- [GitHub Workflows 使用指南](github-workflows-guide.md)

## 文档发布说明

- 预期文档地址：`https://agibuild.github.io/dotnet.CI.template/`
- 工作流会在 `deploy-docs` job 中执行 `dotnet docfx docs/docfx.json`
- 若缺少 `docs/docfx.json` 或未启用 Pages，文档部署会被跳过
