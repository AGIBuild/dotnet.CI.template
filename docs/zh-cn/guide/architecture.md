# 架构设计

本页是承渊的权威架构说明。

如果现有代码与本文档存在差异，后续新增开发应优先遵循本文档，并把代码逐步收敛到这些规则上，而不是继续放大偏差。

## 设计原则

- 先模块，后分层。
- 将可复用切面拆成 `Framework` modules 和 `Application` modules，让技术系统与可复用应用能力分离演进。
- 允许模块深度不一致，不强制所有模块都暴露相同层次或相同传输方式。
- Web 和 CLI 都是可选的 transport facet。
- 所有依赖必须显式声明，并通过架构测试约束。
- 保留强工程约束：构建自动化、锁定还原、版本管理、测试和文档。

## 顶层结构

```text
src/
├── Framework/      # 技术框架模块与 Provider 集成
├── Applications/   # 可复用应用模块以及后续业务模块
├── Hosts/          # 可运行宿主，如 WebHost 和 CliHost
├── ...
tests/              # 单元、集成、架构与模板冒烟测试
build/              # NUKE 构建与打包逻辑
templates/          # dotnet new 模板资源
docs/               # 文档与贡献指南
```

模块家族内部只保留一层模块嵌套：

```text
src/
├── Framework/
│   └── Caching/
│       ├── ChengYuan.Caching.Abstractions/
│       ├── ChengYuan.Caching.Runtime/
│       └── ChengYuan.Caching.Redis/
├── Applications/
│   └── Identity/
│       ├── ChengYuan.Identity.Contracts/
│       ├── ChengYuan.Identity.Domain/
│       ├── ChengYuan.Identity.Application/
│       ├── ChengYuan.Identity.Persistence/
│       ├── ChengYuan.Identity.Web/
│       └── ChengYuan.Identity.Cli/
└── Hosts/
    ├── WebHost/
    └── CliHost/
```

推荐路径形态是 `家族根目录 -> 短模块目录 -> 项目目录`。应使用 `src/Applications/Identity/ChengYuan.Identity.Application/`，而不是 `src/Applications/ChengYuan.Identity/ChengYuan.Identity.Application/`，也不要再引入 `src/Applications/Identity/Application/` 这类只表达角色的中间目录。

测试目录遵循同样的镜像规则。模块测试放在 `tests/ChengYuan.FrameworkKernel.Tests/Applications/Identity/` 这类路径下，架构测试放在 `tests/ChengYuan.ArchitectureTests/Structure/` 这类按测试套件划分的路径下。

`Framework`、`Applications`、`Hosts` 这类家族词只属于目录，不属于项目名。`Application`、`Persistence`、`Web` 这类 facet 词如果是在描述项目职责，则继续保留在项目名中。

## 模块家族

| 家族 | 目的 | 典型例子 | 说明 |
|---|---|---|---|
| `Framework` | 技术系统、抽象、运行时服务、Provider 模型、基础设施桥接 | `Core`、`ExecutionContext`、`Caching`、`Authorization`、`Settings`、`Outbox`、`Hosting` | 不应承载面向用户的业务能力 |
| `Applications` | 可复用应用能力与业务边界上下文 | `Identity`、`PermissionManagement`、`TenantManagement`、`SettingManagement`、后续业务模块 | 可以拥有领域行为、管理用例、持久化和可选传输适配 |
| `Hosts` | 可运行组合壳 | `WebHost`、`CliHost` | 只做模块组合、策略接线和传输胶水 |

`ChengYuan.Core` 是唯一允许承载基础模块化、失败模型、DDD 原语以及共享扩展缝隙的 framework module。`ChengYuan.Hosting` 只保留薄的组合辅助角色，不能继续拥有模块模型本身。

## Facet 模型

每个模块都是垂直切面，但并不是每个切面都必须拥有相同 facet。

### Framework Module 常见 Facet

| Facet | 职责 |
|---|---|
| `Abstractions` | 接口、配置契约、常量、安全共享类型、扩展点 |
| `Runtime` | 框架能力的核心实现 |
| `Provider` | 可替换技术集成，例如 memory、Redis、EF Core、PostgreSQL |
| `AspNetCore` | 可选的 HTTP 或请求管道桥接 |
| `Worker` | 可选的后台处理或轮询 facet |

### Application Module 常见 Facet

| Facet | 职责 |
|---|---|
| `Contracts` | DTO、应用服务接口、权限名、集成事件契约 |
| `Domain` | 聚合、仓储接口、领域服务、策略、领域事件 |
| `Application` | 用例编排、校验、应用服务、事务边界 |
| `Persistence` | 有状态模块的存储实现 |
| `Web` | 可选 HTTP 适配层 |
| `Cli` | 可选 CLI 适配层 |

### 深度不一致是刻意设计

例如：

- `ChengYuan.Caching` 可以是浅层 framework module，只需要 `Abstractions`、`Runtime` 和若干 Provider。
- `ChengYuan.Outbox` 可以只有 `Abstractions`、`Runtime`、`Persistence` 和 `Worker`，不需要 Web 或 CLI。
- `ChengYuan.AuditLogging` 可以先作为无界面的 application module，只包含 `Contracts`、`Application` 和 `Persistence`，以后再补 Web 或 CLI。
- `ChengYuan.Identity` 可以是完整 application module，包含 `Contracts`、`Domain`、`Application`、`Persistence`，并按需增加 Web 或 CLI。

不要为了形式统一去创建空项目。

## 模块配对规则

最核心的规则，是把技术系统与其上层管理能力分开。

| Framework Module | Application Module | 原因 |
|---|---|---|
| `Authorization` | `PermissionManagement` | 权限检查是技术能力；权限授予管理是应用能力 |
| `Settings` | `SettingManagement` | Setting 定义与读取是技术能力；运行时编辑与持久化是应用能力 |
| `MultiTenancy` | `TenantManagement` | Tenant 上下文和解析是技术能力；Tenant 管理是应用能力 |
| `Auditing` | `AuditLogging` | 审计作用域与契约是技术能力；日志持久化与管理是应用能力 |
| `Features` | `FeatureManagement` | Feature 判定是技术能力；Feature 编辑与持久化是应用能力 |

`Identity` 仍然属于 `Applications`，因为它拥有自己的领域行为、管理流程和管理界面，不是单纯技术基础设施。

## Core 基础层设计

基础层边界已经明确：`ChengYuan.Core` 家族专门负责平台级基础能力，旧的 `ChengYuan.Domain` 基线不再属于当前有效架构。

这里遵循的是一组更清晰的规则：

- `Core` 只拥有平台级基础概念。
- Provider 相关能力放在 Core 邻接模块中。
- `ExecutionContext`、`MultiTenancy`、`Caching`、`Outbox` 这类技术系统建立在 `Core` 之上，而不是被折叠进 `Core`。
- 已退场的 `ChengYuan.Domain` 基线不得被重新引入。

### 目标 Core 家族

| 模块 | 负责内容 | 明确不负责 | 依赖 |
|---|---|---|---|
| `ChengYuan.Core` | 基础异常、错误码、`Result`、DDD 原语、强类型 Id、对象扩展契约、`IClock`、`IGuidGenerator` | Json、EF Core、租户上下文、当前用户上下文、缓存、Outbox | 不依赖内部模块 |
| `ChengYuan.Core.Runtime` | 模块描述、模块目录、生命周期钩子、模块引导与排序 | 领域原语、序列化 Provider、数据 Provider | `ChengYuan.Core` |
| `ChengYuan.Core.Json` | 序列化抽象、System.Text.Json 集成、强类型 Id 转换器 | EF Core、仓储或 UoW 逻辑 | `ChengYuan.Core` |
| `ChengYuan.Core.Validation` | 校验契约与默认校验管道 | 本地化资源、持久化 Provider | `ChengYuan.Core` |
| `ChengYuan.Core.Localization` | 资源注册与异常/错误消息本地化缝隙 | 校验运行时策略、持久化 Provider | `ChengYuan.Core` |
| `ChengYuan.Core.Data` | 仓储契约、工作单元契约、数据过滤器、面向持久化的软删除等 trait | EF Core 特定实现细节 | `ChengYuan.Core` |
| `ChengYuan.Core.EntityFrameworkCore` | EF Core 模型约定、值转换器、仓储与 UoW Provider 实现 | Core 抽象本身或非 EF Provider 规则 | `ChengYuan.Core`、`ChengYuan.Core.Data` |

### Core 公开能力的优先顺序

1. 先做模块化：`DependsOnAttribute`、`ModuleBase`、模块描述、模块目录、排序逻辑，以及 pre-configure 和 post-configure 等生命周期钩子。
2. 再做失败模型：`ChengYuanException`、`BusinessException`、稳定的 `ErrorCode`、`Result` 与 `Result<T>`，统一到同一套错误表达之上。
3. 然后是 DDD 原语：`Entity`、`AggregateRoot`、`ValueObject`、领域事件集合、强类型 Id、`IClock`、`IGuidGenerator`。
4. 接着拆出支撑模块：Json、Validation、Localization 成为显式扩展缝隙，而不是散落在 runtime 或 host 里。
5. 再引入数据抽象：仓储、工作单元、软删除和多租户等过滤器接入点。
6. 最后再做 Provider：EF Core 约定、强类型 Id 转换器、仓储、过滤器与持久化辅助能力。

### 当前到目标的迁移映射

| 当前位置 | 目标位置 | 迁移规则 |
|---|---|---|
| `ChengYuan.Hosting` 中的模块原语 | `ChengYuan.Core.Runtime` | `Hosting` 不再拥有模块化模型，只保留薄的组合辅助职责 |
| `ChengYuan.Domain` 中的 Result 与强类型 Id 原语 | `ChengYuan.Core` | 核心原语迁移到明确的 Core 命名空间，不再伪装成单独的 Domain 模块 |
| `ChengYuan.Domain` 中的 Json 转换器 | `ChengYuan.Core.Json` | 序列化支持成为 Core 邻接模块，而不是 core runtime 的一部分 |
| `ChengYuan.Domain.EntityFrameworkCore` 中的转换器 | `ChengYuan.Core.EntityFrameworkCore` | EF Core 支持依赖 Core，Core 绝不能反向依赖 EF Core |
| `ExecutionContext.IClock` | `ChengYuan.Core` | 先把抽象提升到 Core，运行时实现保持可迁移 |
| `MultiTenancy` 的 tenant 契约 | 保持在 `ChengYuan.MultiTenancy` | 后续通过 `Core.Data` 过滤器接入，而不是把租户上下文并入 Core |

### 实施波次

1. Wave A：创建 `ChengYuan.Core` 家族，并把模块化所有权从 `ChengYuan.Hosting` 迁出。
2. Wave B：建立 Core 的失败模型和 DDD 基线，包括异常、错误码、`Result`、实体、聚合根、值对象、强类型 Id、`IClock`、`IGuidGenerator`。
3. Wave C：增加 `ChengYuan.Core.Json`、`ChengYuan.Core.Validation`、`ChengYuan.Core.Localization`、`ChengYuan.Core.Data`，并用架构测试约束依赖边界。
4. Wave D：增加 `ChengYuan.Core.EntityFrameworkCore`，承载转换器、仓储、工作单元和数据过滤器实现。
5. Wave E：将 `ExecutionContext`、`MultiTenancy`、`Caching`、`Outbox`，以及后续的 `Authorization`、`Settings`、`Features`、`Auditing` 重建到新的 Core 家族之上。
6. Wave F：把测试拆分并收紧为 core modularity、core primitives、provider、framework-kernel 四类。
7. Wave G：在全部引用迁移完成后，正式移除旧的 `ChengYuan.Domain` 命名。已完成。

### 架构护栏

- `Core` 不得依赖 `ExecutionContext`、`MultiTenancy`、`Caching`、`Outbox`、Json runtime 或 EF Core。
- `Hosting` 不得继续拥有模块基类、模块描述或生命周期契约。
- Json 和 EF Core 支持必须作为显式可选模块依赖存在，不能隐藏在 `Core` 内部自动生效。
- 数据层默认仓储只服务于聚合根。
- `CurrentUser` 和 `CurrentTenant` 保持在各自技术系统中，不迁入 `Core`。
- 第一轮 Core 重构不引入大包堆叠、动态代理、自动生成 API 层或 UI 框架层。

## Phase 1 模块目录

### Framework 家族

- `ChengYuan.Core`
- `ChengYuan.Core.Runtime`
- `ChengYuan.Core.Json`
- `ChengYuan.Core.Validation`
- `ChengYuan.Core.Localization`
- `ChengYuan.Core.Data`
- `ChengYuan.Core.EntityFrameworkCore`
- `ChengYuan.Hosting`
- `ChengYuan.ExecutionContext`
- `ChengYuan.MultiTenancy`
- `ChengYuan.Authorization`
- `ChengYuan.Settings`
- `ChengYuan.Auditing`
- `ChengYuan.Caching`
- `ChengYuan.Outbox`

### Applications 家族

- `ChengYuan.Identity`
- `ChengYuan.PermissionManagement`
- `ChengYuan.TenantManagement`
- `ChengYuan.SettingManagement`
- `ChengYuan.AuditLogging`
- `ChengYuan.FeatureManagement` 作为后续阶段能力，在第一版骨架稳定后再引入

后续由模板生成的业务模块，也统一放在 `src/Applications`。

## 依赖规则

- `Framework` modules 只能依赖其他 `Framework` modules 和被批准的外部库。
- 所有 `Framework` 与 `Application` modules 都可以依赖 `ChengYuan.Core`。
- 只有宿主和参与模块引导的 framework runtime facet 才允许依赖 `ChengYuan.Core.Runtime`。
- `Application` modules 可以依赖 `Framework` modules，也只能通过 `Contracts` 依赖其他 `Application` modules。
- 任何模块都不能依赖别的模块的 `Persistence`、`Web` 或 `Cli` facet。
- `Web` 和 `Cli` facet 应通过 DI 解析应用服务，只依赖契约，不直接依赖实现项目。
- Hosts 只允许依赖 framework runtime facet 与被选中的 application transport facet。
- 跨模块读取优先使用 lookup 或 integration service 契约。
- 跨模块副作用优先在模块化单体中使用 local events，并为未来 distributed events 保留路径。
- Provider 与 contributor 模式应归属其所属的 framework 或 application module，而不是散落在 host 中。
- Core 重构开始后，不允许再通过 `Hosting` 反向依赖模块化原语。

## Host 组合模型

### Web Host

`ChengYuan.WebHost` 是 API-first 的 ASP.NET Core 外壳，负责组合：

- 被选中的 framework runtime facet
- 被选中的 application `Web` facet
- 中间件、认证接线、健康检查和传输策略

它不拥有应用用例。

### CLI Host

`ChengYuan.CliHost` 是基于 System.CommandLine 和 Spectre.Console 的命令行外壳，负责组合：

- 被选中的 framework runtime facet
- 被选中的 application `Cli` facet
- 控制台交互与脚本友好的输出

CLI 场景可以有意省略某些与命令行无关的模块。认证型模块、重 UI 模块，甚至某些持久化较重的模块都可以按需选择，只要声明依赖被满足即可。

## 后续开发规则

1. 在创建任何项目之前，先决定模块属于哪个家族。
2. 选择满足用例所需的最小 facet 集合。
3. 不要为了形式统一而创建空的 `Web`、`Cli` 或 `Persistence` 项目。
4. 目录结构固定为 `家族根目录 -> 短模块目录 -> 项目目录`。
5. 项目名中不要带家族词，但描述项目职责的 facet 词应保留。
6. 每次引入新模块或新 facet，都补充架构测试。
7. 重复出现的技术能力应提升到 `Framework`，而不是塞进共享应用项目。
8. 可复用管理能力应进入 `Applications`，而不是堆到 host 中。
9. Hosts 必须保持薄且声明式。

## 实施默认策略

- phase 1 先用一个浅层 framework module，如 `Caching`，一个有状态 framework module，如 `Outbox`，以及一个完整 application module，如 `Identity`，来验证“深度不一致”的设计是否稳定。
- 在继续扩展更多 framework 或 management modules 之前，保持 `ChengYuan.Core` 的所有权切分稳定，不要重新引入已经退场的 `ChengYuan.Domain` 基线。
- 当这三种形态稳定后，再扩展其余 framework 和 application families。

## 相关页面

- [产品介绍](./introduction.md)
- [开发环境](../contributing/development.md)