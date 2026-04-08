# 开发环境

## 先看架构

在新增任何项目、包引用或传输适配层之前，先阅读 [架构设计](../guide/architecture.md)。该页面是后续开发的权威设计说明。

如果当前源码与架构文档存在差异，应优先遵循文档，并让新开发朝文档定义的规则收敛。

## 核心开发规则

- 先决定模块属于 `Framework` 还是 `Applications`。
- 选择满足用例所需的最小 facet 集合，不要为了对称性创建空的 `Web`、`Cli` 或 `Persistence` 项目。
- 拓扑词只放在目录中，不放在项目名中。
- `Framework` modules 只能依赖其他 `Framework` modules。
- `Application` modules 可以依赖 `Framework` modules，也只能通过 `Contracts` 依赖其他 `Application` modules。
- Hosts 只做模块组合，不承载业务用例实现。
- 引入新模块或新 facet 形态时，必须补充架构测试。

## 前置要求

| 工具 | 版本 | 备注 |
|---|---|---|
| .NET SDK | 10.0+ | 由 `global.json` 锁定（`rollForward: latestFeature`） |
| Git | 2.x | |
| Node.js | 18+ | 仅文档开发需要（`docs/package.json`） |

## 克隆与构建

```bash
git clone <repo-url>
cd <repo-folder>

./build.sh          # Linux / macOS
build.ps1           # Windows
```

默认 target 是 **Build**（Restore → Build）。

## 模块开发清单

1. 先确定模块家族。
2. 再确定模块 profile 或 facet 组合。
3. 只创建模块真正需要的项目。
4. 显式声明直接依赖。
5. 添加或更新架构测试。
6. 只有在需要对应传输方式时，才把模块接入 Web 或 CLI。

## NUKE 构建 Target

所有构建逻辑在 `build/` 目录。Workflow 调用这些 target，而不是直接执行 `dotnet` 命令。

| Target | 依赖 | 说明 |
|---|---|---|
| **Restore** | — | `dotnet restore` + 工具还原 |
| **Build** | Restore | 构建解决方案 |
| **Test** | Build | 运行测试并收集覆盖率（trx + Coverlet） |
| **Pack** | Test | 打包 NuGet（.nupkg + .snupkg） |
| **Publish** | Restore | 为指定 `--Host` 和 `--Runtime` 发布 Host |
| **PackageApp** | Publish | 将发布产物打成 `app-{host}-{runtime}.zip` |
| **Format** | Restore | `dotnet format --verify-no-changes` |
| **CoverageReport** | Test | 生成 HTML + Cobertura 覆盖率报告 |
| **ShowVersion** | — | 打印当前 `VersionPrefix` |
| **UpdateVersion** | — | 递增 patch 或显式设置 `--VersionPrefix` |
| **GenerateReleaseManifest** | Pack | 创建含 SHA256 哈希的 `release-manifest.json` |

### 常用命令

```bash
./build.sh Test                                   # 构建 + 测试
./build.sh Pack                                   # 构建 + 测试 + 打包
./build.sh Publish --Host web --Runtime linux-x64 --SelfContained  # 发布自包含 Web Host
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
| `--Host` | — | 要发布的 Host（`web` 或 `cli`） |
| `--Runtime` | — | 目标 RID（如 `linux-x64`） |
| `--SelfContained` | `false` | 是否生成自包含产物 |

### 产物目录

所有输出位于 `artifacts/`：

```text
artifacts/
├── test-results/   # .trx 文件 + 覆盖率
├── packages/       # .nupkg + .snupkg + release-manifest.json
├── publish/        # 按 host/runtime 分组的 dotnet publish 产物
└── installers/     # app-{host}-{runtime}.zip
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

- [架构设计](../guide/architecture.md) — 仓库级设计规则与模块边界。
- [CI/CD 流程](ci-cd.md) — GitHub Actions 流水线如何运作。
- [发版指南](releasing.md) — 如何发布新版本。
