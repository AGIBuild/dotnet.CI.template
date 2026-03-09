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

- `mode`: `create_and_release`
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

确认 `NUGET_API_KEY` 已配置后，再次运行 `Release`：

- `mode`: `release_existing_tag`
- `tag`: 例如 `v0.1.0`
- `publish_nuget`: `true`

> `tag` 必须是 `v<semver>` 格式，例如 `v1.2.3`、`v1.2.3-rc.1`。

---

## 4) 关于版本的两条规则（必读）

- 触发 `create_and_release` 时不需要也不能手填 `version`，版本来自 `Directory.Build.props` 的 `VersionPrefix`。
- 如果要升级主版本（例如 `0.x -> 1.0.0`），先通过 PR 修改 `VersionPrefix` 并合并到 `main`，再触发 `Release`。  
  `Release` 不会自动修改仓库文件，它只基于现有提交打 tag 和发版。

---

## 5) 常见失败快速定位

- `Tag already exists`: 版本已发过，改版本或使用 `release_existing_tag`
- `Invalid tag format`: tag 不是 `v<semver>`
- `No .nupkg files found`: 打包阶段没有产物，先看 `Build/Test/Pack` 日志
- `publish_nuget requested but NUGET_API_KEY...`: 未配置 NuGet secret

---

## 6) CLI 触发（可选）

```bash
# 创建并发布（不推 NuGet）
gh workflow run release.yml --ref main -f mode=create_and_release -f publish_nuget=false

# 用已有 tag 推 NuGet
gh workflow run release.yml --ref main -f mode=release_existing_tag -f tag=v0.1.0 -f publish_nuget=true
```

---

进阶说明见：`docs/github-workflows-guide.md`。
