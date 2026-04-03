# 产品介绍

承渊是当前仓库的目标产品方向。

它是一个面向模块化单体的 .NET 模板家族，以 ABP 风格的模块边界、DDD+ 以及严格的工程约束为核心。仓库正在从旧的 CI 模板形态迁移到这套架构模型，所以当前实现中仍可能保留旧结构。后续新增开发应优先以本文档描述的目标架构为准。

## 产品方向

承渊围绕以下五个核心思想设计：

- **模块优先** — 每个可复用能力先被设计成垂直切面。
- **双家族模型** — 用 `Framework` modules 承载技术系统，用 `Application` modules 承载可复用能力和业务边界。
- **可组合宿主** — Web 和 CLI 都是只加载所需 facet 的薄宿主。
- **模块深度可变** — 并非每个模块都需要 Web、CLI、Persistence 或 UI。
- **工程纪律不可退让** — 构建自动化、锁文件、版本管理、测试和文档必须保留。

## 目标拓扑

```text
src/
├── Framework/      # 技术框架模块与 Provider 集成
├── Applications/   # 可复用应用模块与业务模块
├── Hosts/          # WebHost 和 CliHost 外壳
├── ...
tests/              # 分层、集成、架构与模板测试
build/              # NUKE 构建自动化
docs/               # 文档
```

源码目录中的推荐结构是：

```text
family -> module -> project
```

例如，更推荐 `src/Applications/ChengYuan.Identity/ChengYuan.Identity.Application/`，而不是把所有模块项目拍平到同一层目录。

## 承渊不是什么

- 它不是仓库级的横向 clean architecture 骨架。
- 它不是完整照搬 ABP 的运行时框架。
- 它不是微服务优先。
- 它不假设所有模块都必须暴露完全相同的层次和传输方式。

## 建议接着阅读

- [架构设计](./architecture.md) — 后续开发必须遵循的权威设计规则。
- [快速开始](./getting-started.md) — 当前模板资产的使用方式。
- [开发环境](../contributing/development.md) — 本地开发流程、构建命令与贡献规则。
