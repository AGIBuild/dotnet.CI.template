# Quick Start: Release in 2 Minutes

适用对象：第一次接手这个仓库的新同事。  
目标：**快速完成一次标准 Release**。

---

## 0) 前置条件（30 秒）

- 你有仓库写权限
- `main` 分支是最新、`CI` 通过
- 如果要推 NuGet，仓库已配置 `NUGET_API_KEY` secret

---

## 1) 标准发版（推荐）

在 GitHub Actions 里手动运行 `Release`：

- `publish_nuget`: `false`（首次建议先关闭）

然后点击 **Run workflow**。

---

## 2) 成功标准（30 秒检查）

运行成功后应看到：

- 新 tag（例如 `v0.1.0`）
- 一个新的 GitHub Release
- Release 附件里有 `.nupkg` / `.snupkg`

---

## 3) 要发 NuGet 怎么做

确认 `NUGET_API_KEY` 已配置后，在运行 `Release` 时直接设置：

- `publish_nuget`: `true`

> 当前 workflow 不支持使用已存在 tag 触发。  
> 如果某个版本已发布且当时未推 NuGet，需要先提升 `VersionPrefix` 后再发下一个版本。

---

## 4) 关于版本的两条规则（必读）

- 触发 `create_and_release` 时不需要也不能手填 `version`，版本来自 `Directory.Build.props` 的 `VersionPrefix`。
- 触发 `Release` 时不需要也不能手填 `version` 或 `tag`，版本来自 `Directory.Build.props` 的 `VersionPrefix`。
- 如果要升级主版本（例如 `0.x -> 1.0.0`），先通过 PR 修改 `VersionPrefix` 并合并到 `main`，再触发 `Release`。  
  `Release` 不会自动修改仓库文件，它只基于现有提交打 tag 和发版。

---

## 5) 常见失败快速定位

- `Tag already exists`: 版本已发过，先提升 `VersionPrefix` 再重试
- `No .nupkg files found`: 打包阶段没有产物，先看 `Build/Test/Pack` 日志
- `publish_nuget requested but NUGET_API_KEY...`: 未配置 NuGet secret

---

## 6) CLI 触发（可选）

```bash
# 创建并发布（不推 NuGet）
gh workflow run release.yml --ref main -f publish_nuget=false

# 创建并发布（推 NuGet）
gh workflow run release.yml --ref main -f publish_nuget=true
```

---

进阶说明见：`docs/github-workflows-guide.md`。
