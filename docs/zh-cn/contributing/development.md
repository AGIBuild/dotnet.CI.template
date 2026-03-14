# 开发环境

## 前置要求

| 工具 | 版本 | 备注 |
|---|---|---|
| .NET SDK | 10.0+ | 由 `global.json` 锁定（`rollForward: latestFeature`） |
| Git | 2.x | |
| Node.js | 18+ | 仅文档开发需要（`docs/package.json`） |

## 克隆与构建

```bash
git clone https://github.com/AGIBuild/dotnet.CI.template.git
cd dotnet.CI.template

./build.sh          # Linux / macOS
build.ps1           # Windows
```

默认 target 是 **Build**（Restore → Build）。

## NUKE 构建 Target

所有构建逻辑在 `build/` 目录。Workflow 调用这些 target，而不是直接执行 `dotnet` 命令。

| Target | 依赖 | 说明 |
|---|---|---|
| **Restore** | — | `dotnet restore` + 工具还原 |
| **Build** | Restore | 构建解决方案 |
| **Test** | Build | 运行测试并收集覆盖率（trx + Coverlet） |
| **Pack** | Test | 打包 NuGet（.nupkg + .snupkg） |
| **Publish** | Restore | 为指定 `--Runtime` 发布应用 |
| **PackageApp** | Publish | 将发布产物打成 `app-{runtime}.zip` |
| **Format** | Restore | `dotnet format --verify-no-changes` |
| **CoverageReport** | Test | 生成 HTML + Cobertura 覆盖率报告 |
| **ShowVersion** | — | 打印当前 `VersionPrefix` |
| **UpdateVersion** | — | 递增 patch 或显式设置 `--VersionPrefix` |
| **GenerateReleaseManifest** | Pack | 创建含 SHA256 哈希的 `release-manifest.json` |

### 常用命令

```bash
./build.sh Test                                   # 构建 + 测试
./build.sh Pack                                   # 构建 + 测试 + 打包
./build.sh Publish --Runtime linux-x64 --SelfContained  # 发布自包含应用
./build.sh CoverageReport                         # 生成覆盖率 HTML
./build.sh ShowVersion                            # 查看版本号
./build.sh UpdateVersion                          # patch 递增（如 0.2.0 → 0.2.1）
./build.sh UpdateVersion --VersionPrefix 1.0.0    # 显式设置版本
```

### 参数

| 参数 | 默认值 | 说明 |
|---|---|---|
| `--Configuration` | `Debug`（本地）/ `Release`（CI） | 构建配置 |
| `--VersionPrefix` | — | 要设置的版本号（用于 `UpdateVersion`） |
| `--VersionSuffix` | — | 预发布后缀（如 `ci.42`） |
| `--Runtime` | — | 目标 RID（如 `linux-x64`） |
| `--SelfContained` | `false` | 是否生成自包含产物 |

### 产物目录

所有输出位于 `artifacts/`：

```text
artifacts/
├── test-results/   # .trx 文件 + 覆盖率
├── packages/       # .nupkg + .snupkg + release-manifest.json
├── publish/        # dotnet publish 产物
└── installers/     # app-{runtime}.zip
```

## 文档开发

```bash
cd docs
npm install
npm run docs:dev      # 本地开发服务器 http://localhost:5173
npm run docs:build    # 生产构建
npm run docs:preview  # 预览生产构建
```

## 下一步

- [CI/CD 流程](ci-cd.md) — GitHub Actions 流水线如何运作。
- [发版指南](releasing.md) — 如何发布新版本。
