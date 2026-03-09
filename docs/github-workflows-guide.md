# GitHub Workflows 使用指南

这套 workflow 的目标很直接：  
**提交就有质量反馈，发版不靠手工，产物可追溯，可重复。**

如果你刚从这个模板创建仓库，可以把它当成一条标准流水线：

```text
push / PR
  ├─ CI (build + test)
  └─ CodeQL (security scan)

manual dispatch
  ├─ Release (resolve -> build/test/pack -> release -> optional nuget push)
  └─ App Publish Artifacts (matrix publish by runtime)
```

快速发版可先看：`docs/quick-start-release.md`。

---

## 1) 先认识 4 条主 workflow

### `CI`
- 触发：`push` 到 `main`，或对 `main` 的 `pull_request`
- 作用：执行 `./build.sh Test`（模板里走 NUKE 目标）
- 产物：测试结果（`artifacts/test-results/**`）

### `Release`
- 触发：手动 `workflow_dispatch`
- 作用：解析版本/标签 -> build/test/pack -> 创建 GitHub Release -> 可选推送 NuGet
- 特点：带版本一致性校验，避免 tag 与包版本漂移

### `App Publish Artifacts`
- 触发：手动 `workflow_dispatch`
- 作用：按 RID 矩阵发布应用产物（`win-x64` / `linux-x64` / `osx-arm64` 等）
- 场景：你要发布“可运行应用”而不是 NuGet 包时

### `CodeQL`
- 触发：`push`/`pull_request` 到 `main`，以及定时任务
- 作用：安全分析（C#）
- 特点：同样通过 `./build.sh Build` 走统一构建入口

---

## 2) 最快上手路径（推荐）

1. 提交一次代码到 `main`  
   观察 `CI` 和 `CodeQL` 是否都绿。

2. 手动触发一次 `Release`
   - `mode=create_and_release`
   - `publish_nuget=false`（先做演练）

3. 去 Releases 页面确认
   - 已创建 tag（如 `v0.1.0`）
   - 已生成 `.nupkg` / `.snupkg` 资产

---

## 3) Release 输入参数怎么选

### `mode`
- `create_and_release`：从 `Directory.Build.props` 读取 `VersionPrefix`，自动生成 tag 并发布
- `release_existing_tag`：使用你传入的 `tag`（如 `v1.2.3`）进行发布

### `tag`
- 仅在 `release_existing_tag` 模式下必填
- 格式必须是 `v<semver>`（例如 `v1.2.3`、`v1.2.3-rc.1`）

### `publish_nuget`
- `true`：若仓库配置了 `NUGET_API_KEY`，会执行 NuGet 推送
- `false`：只创建 GitHub Release，不推送 NuGet

---

## 4) 版本机制 FAQ（重点）

### Q1: 触发 `Release` 时“未指定版本”会怎样？
- 当前 workflow 没有 `version` 输入参数。
- `create_and_release` 模式会从 `Directory.Build.props` 读取 `VersionPrefix` 作为版本。
- 如果对应 tag 已存在（例如 `VersionPrefix=0.1.0` 且 `v0.1.0` 已存在），运行会失败并报 `Tag already exists`。
- `release_existing_tag` 模式如果未提供 `tag`，会直接失败并报 `Tag input is required`。

### Q2: Release 时如果要改主版本，怎么反映到最新代码？
- 正确方式是先改代码：通过 PR 修改 `Directory.Build.props` 里的 `VersionPrefix`（例如 `0.1.0 -> 1.0.0`），并合并到 `main`。
- 再触发 `Release` 的 `create_and_release`，它会基于当前 `main` 的最新提交打 tag 并发布。
- 也就是说，**版本变更通过代码提交生效**；`Release` 本身不会回写或修改仓库文件。

---

## 5) App Publish Artifacts 典型用法

常见输入建议：
- `configuration`: `Release`
- `run_tests`: `true`
- `self_contained`: `false`（框架依赖，更轻）
- `runtimes`: `win-x64,linux-x64,osx-arm64`

如果你要离线分发单文件部署，可把 `self_contained` 设为 `true`，并按需精简 `runtimes`。

---

## 6) 用命令行触发（可选）

```bash
# 触发 release（自动创建 tag + release，不推 NuGet）
gh workflow run release.yml --ref main -f mode=create_and_release -f publish_nuget=false

# 用已有 tag 触发 release
gh workflow run release.yml --ref main -f mode=release_existing_tag -f tag=v1.2.3 -f publish_nuget=false

# 触发应用产物发布
gh workflow run app-publish-artifacts.yml --ref main -f configuration=Release -f run_tests=true -f self_contained=false -f runtimes=win-x64,linux-x64
```

---

## 7) 常见问题排查（先看这里）

- `Release` 报 tag 已存在：检查是否重复发布同一 `VersionPrefix`
- `publish_nuget=true` 但未推送：确认仓库是否配置 `NUGET_API_KEY`
- 包版本校验失败：检查 `Directory.Build.props` 的 `VersionPrefix` 与目标 tag 是否一致
- Windows/Linux 行为不一致：确认 `.gitattributes` 已生效，特别是 `*.sh` 的 LF

---

## 8) 团队协作建议

- 日常开发只关注：`CI`、`CodeQL`
- 发版动作统一从 `Release` 入口走，避免手工打包/手工传产物
- 变更 workflow 逻辑时，优先扩展 NUKE 目标，再回到 workflow 编排

这样团队会得到一个很稳定的体验：  
**开发快、反馈快、发布稳。**
