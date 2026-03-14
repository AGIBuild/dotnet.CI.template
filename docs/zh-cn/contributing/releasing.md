# 发版指南

适用对象：需要发布新版本的团队成员。
目标：**快速完成一次标准 Release**。

---

## 0) 前置条件

- 你有仓库写权限
- `main` 分支是最新、CI 通过
- 如果要推 NuGet，仓库已配置 `NUGET_API_KEY` secret
- GitHub 仓库已创建 `release` environment（Settings → Environments），并配置 required reviewers

---

## 1) Release 流程概览

Release 集成在 `CI and Release` workflow 中，由 5 个 job 组成：

```text
                ┌─ build-and-test (matrix) → release (需 approve) ─┐
resolve-version─┤                                                   ├→ deploy-docs
                └─ build-docs (前置检查) ──────────────────────────┘
```

当代码 push 到 `main` 后：
1. CI 自动构建、测试、打包（NuGet 包 + 平台安装包 zip）
2. `release` job 等待 `release` environment 的 **reviewer approval**
3. Approve 后自动完成：NuGet 推送 → SBOM 生成 + Artifact Attestation → 创建 tag + GitHub Release（安装包 zip + SBOM） → 文档部署

---

## 2) 如何触发 Release

**自动触发**：合并 PR 到 `main` 后，CI 自动运行。构建完成后进入 approval 等待。前往 Actions 页面找到对应的 workflow run，点击 **Review deployments** 审批 `release` environment。

**手动触发**：在 Actions 页面手动 **Run workflow**（选择 `main` 分支）。

---

## 3) 成功标准

运行成功后应看到：

- 新 tag（例如 `v0.2.0`）
- 一个新的 GitHub Release
- Release 附件里有各平台安装包 zip（`app-linux-x64.zip`、`app-win-x64.zip` 等）和 SBOM 文件
- NuGet.org 上有对应版本的包（如已配置 `NUGET_API_KEY`）
- 如果已启用 Pages：文档自动部署

---

## 4) 版本管理（必读）

版本来自 `Directory.Build.props` 的 `VersionPrefix`——纯 3 段式 SemVer（如 `0.2.0`）。只有当该版本与最新 git tag 不同时才会触发 release。

```bash
./build.sh ShowVersion                           # 查看当前版本
./build.sh UpdateVersion                         # patch 递增: 0.2.0 -> 0.2.1
./build.sh UpdateVersion --VersionPrefix 1.0.0   # 精确设置
```

修改后提交到 `main`，CI 自动基于新版本构建和发布。

---

## 5) 常见失败快速定位

- `Tag already exists`: 版本已发过，先提升 `VersionPrefix` 再重试
- `No .nupkg files found`: 打包阶段没有产物，先看 Build/Test/Pack 日志
- `Hash mismatch`: 构建产物在传递过程中损坏，重新触发 workflow
- NuGet 推送失败: 确认 `NUGET_API_KEY` secret 已配置

---

## 6) CLI 触发（可选）

```bash
# 手动触发 CI and Release workflow
gh workflow run ci.yml --ref main
```

---

进阶说明见：[CI/CD 流程](ci-cd.md)。
