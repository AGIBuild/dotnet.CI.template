# GitHub Workflows 使用指南

这套 workflow 的目标很直接：  
**提交就有质量反馈，发版不靠手工，产物可追溯，可重复。**

如果你刚从这个模板创建仓库，可以把它当成一条标准流水线：

```text
push / PR to main
  └─ CI and Release
       ├─ resolve-version
       ├─ build-and-test (matrix: PR=linux, main=全平台)
       ├─ release-packages (需 approval，推 NuGet)
       ├─ deploy-docs (GitHub Pages)
       └─ create-release (tag + GitHub Release)

push / PR to main + weekly
  └─ CodeQL (security scan)
```

快速发版可先看：`docs/quick-start-release.md`。

---

## 1) 先认识 2 条 workflow

### `CI and Release`
- 触发：`push` 到 `main`、对 `main` 的 `pull_request`、手动 `workflow_dispatch`
- PR 行为：只在 ubuntu 上运行 Build + Test（带 prerelease suffix）
- main push 行为：全平台矩阵 Build + Test + Pack + Publish → approval → NuGet 推送 → 文档部署 → GitHub Release
- 产物：测试结果、NuGet 包（含 release manifest）、各平台安装包

### `CodeQL`
- 触发：`push`/`pull_request` 到 `main`，以及每周定时任务
- 作用：安全分析（C#）
- 特点：通过 `./build.sh Build` 走统一构建入口

---

## 2) Job 详解

### `resolve-version`
- 从 `Directory.Build.props` 读取 `VersionPrefix`，验证 semver 格式
- 判断是否为 release（main push = true，PR = false）
- 计算构建矩阵：PR 仅 ubuntu，main push 包含 win/linux/osx

### `build-and-test`
- 矩阵构建：各平台执行 Build + Test
- PR：带 `--VersionSuffix "ci.<run_number>"`
- main push：无 suffix（固化 release 版本）
- linux runner 额外执行 Pack + GenerateReleaseManifest（生成 SHA256 manifest）
- 各平台执行 Publish 生成安装包

### `release-packages`
- 需要 `release` environment approval（唯一的审批入口）
- 下载 NuGet 包，验证 release manifest SHA256 完整性
- 推送 NuGet 包到 nuget.org

### `deploy-docs`
- 依赖 `release-packages` 成功后自动运行
- 如果存在 `docs/docfx.json`，构建 DocFX 并部署到 GitHub Pages
- 如果不存在则跳过

### `create-release`
- 依赖 `release-packages` 和 `deploy-docs` 完成
- 创建 git tag 并推送到 remote
- 创建 GitHub Release，附带所有产物（NuGet 包 + 安装包 zip）

---

## 3) 最快上手路径（推荐）

1. 提交一次代码到 `main`  
   观察 `CI and Release` 和 `CodeQL` 是否都绿。

2. 前往 Actions → 找到对应的 workflow run → 点击 **Review deployments**  
   审批 `release` environment。

3. 去 Releases 页面确认：
   - 已创建 tag（如 `v0.1.0`）
   - 已生成 `.nupkg` / `.snupkg` + 安装包 zip

---

## 4) Environment 配置（必须）

### `release` environment
在 GitHub 仓库 Settings → Environments → 新建 `release`：
- **Required reviewers**：至少添加一个 reviewer
- 可选：配置 wait timer、deployment branches（限制为 `main`）

### `github-pages` environment（可选）
如果使用 DocFX 文档部署：
- Settings → Pages → Source 选择 GitHub Actions
- environment `github-pages` 会自动创建

### Secrets
- `NUGET_API_KEY`：NuGet.org API key（在 repo 或 `release` environment 级别配置）

---

## 5) 版本机制 FAQ（重点）

### Q1: 版本从哪里来？
版本来自 `Directory.Build.props` 的 `VersionPrefix`。CI 不接受手动输入版本参数。

### Q2: 如何升级版本？
通过 PR 修改 `VersionPrefix`（例如 `0.1.0 -> 0.2.0`），合并到 `main` 后 CI 自动基于新版本构建。

### Q3: 同一版本能否重新发布？
不能。如果 tag 已存在，`create-release` 会直接失败。需要先提升 `VersionPrefix`。

### Q4: Release manifest 是什么？
`release-manifest.json` 记录每个 NuGet 包的 SHA256 hash 和版本信息。发布阶段会验证包文件与 manifest 一致，防止产物在传递过程中被篡改或损坏。

---

## 6) 用命令行触发（可选）

```bash
# 手动触发 CI and Release
gh workflow run ci.yml --ref main
```

---

## 7) 常见问题排查（先看这里）

- tag 已存在：说明同版本已发布；请先通过 PR 提升 `VersionPrefix`
- NuGet 推送失败：确认仓库是否配置 `NUGET_API_KEY`
- 包版本校验失败：检查 `Directory.Build.props` 的 `VersionPrefix` 是否正确
- Hash mismatch：产物在 job 间传递时损坏，重新触发 workflow
- Windows/Linux 行为不一致：确认 `.gitattributes` 已生效，特别是 `*.sh` 的 LF
- 文档部署跳过：正常行为，需要先配置 `docs/docfx.json`

---

## 8) 团队协作建议

- 日常开发只关注 CI 是否绿灯（Build + Test + CodeQL）
- 发版通过 environment approval 统一管控，避免手工打包/手工传产物
- 变更 workflow 逻辑时，优先扩展 NUKE 目标，再回到 workflow 编排

这样团队会得到一个很稳定的体验：  
**开发快、反馈快、发布稳。**
