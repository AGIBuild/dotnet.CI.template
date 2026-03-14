# 产品介绍

Dotnet.CI.Template 是一个 GitHub 模板仓库，为新 .NET 项目提供生产级起点：

- **NUKE 构建系统** — 用类型安全的 C# 构建 target 取代脆弱的 shell 脚本。
- **GitHub Actions CI/CD** — 全平台构建、测试、打包、发布和发版在同一条流水线完成。
- **版本管理** — `Directory.Build.props` 中的 `VersionPrefix` 是唯一版本来源；CI 根据 git tag 自动判断是否需要发版。
- **文档站点** — 支持多语言的 VitePress 站点（中文 + 英文），自动构建并部署到 GitHub Pages。

## 产物一览

| 产物 | 说明 |
|---|---|
| NuGet 包 | 含符号包和 release manifest 的类库 |
| 平台安装包 | 自包含应用归档（`app-{runtime}.zip`） |
| SBOM | SPDX JSON 格式的软件物料清单 |
| 文档站点 | 部署到 GitHub Pages 的静态站 |

## 项目结构

```text
├── src/                    # 源码项目
├── tests/                  # 测试项目
├── build/                  # NUKE 构建 target
├── docs/                   # VitePress 文档
├── .github/workflows/      # CI/CD 流水线
├── Directory.Build.props   # 共享构建属性与版本号
└── Dotnet.CI.Template.slnx # 解决方案文件
```

## 下一步

- [快速开始](getting-started.md) — 安装和使用库。
- [开发环境](../contributing/development.md) — 搭建本地开发环境。
