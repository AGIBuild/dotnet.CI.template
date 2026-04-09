# API 参考

## 当前公开接口面

### `ChengYuan.Core`

基础结果模型、标识与 DDD 原语。

#### 关键类型

| 类型 | 说明 |
|---|---|
| `Result` 与 `Result<TValue>` | 供应用层与框架层共享的成功/失败模型。 |
| `ResultError` | 带错误码、消息与失败类别的稳定错误形状。 |
| `CoreRuntimeModule` | 注册扩展属性定义注册表等共享 Core 服务的基础运行时模块。 |
| `ErrorCode` | 用于稳定框架与业务错误码的小型值对象。 |
| `ChengYuanException` | 供框架层与应用层使用、可携带错误码元数据的基础异常。 |
| `BusinessException` | 可桥接到共享 `ResultError` 形状的业务异常类型。 |
| `Check` | 集中的 guard helper，负责空值、默认值、范围、Guid 与集合校验。 |
| `StronglyTypedId<TValue>` | 框架内使用的类型安全 Id 基类。 |
| `Entity<TId>` | 基于身份相等性的实体基类。 |
| `AggregateRoot<TId>` | 同时跟踪领域事件的实体基类。 |
| `ValueObject` | 提供结构化相等性的值对象基类。 |
| `IHasExtraProperties` | 向实体或模型暴露额外属性袋的契约。 |
| `ExtraPropertyDictionary` | 供对象扩展模型使用的字符串键属性袋。 |
| `ExtraPropertyExtensions` | 读写额外属性的强类型辅助方法。 |
| `ExtraPropertyDefinition` | 带类型、默认值、必填标记与元数据的扩展属性定义。 |
| `ExtraPropertyDefinitionBuilder` | 用于配置扩展属性默认值与元数据的 fluent builder。 |
| `ExtraPropertyManager` | 按运行时类型解析扩展属性定义的注册表。 |
| `AddCoreRuntime()` | 向 DI 容器注册默认 Core 运行时服务。 |
| `IDomainEvent` | 聚合领域事件的标记抽象。 |
| `IClock` | 运行时与基础设施代码使用的时间抽象。 |
| `IGuidGenerator` | 由框架控制 Guid 生成的抽象接口。 |

### `ChengYuan.Core.Runtime`

模块组合核心原语。

#### 关键类型

| 类型 | 说明 |
|---|---|---|
| `ModuleBase` | Framework 与 Application 模块的基类。 |
| `DependsOnAttribute` | 声明直接模块依赖，用于组合排序。 |
| `ModuleCatalog` | 暴露 Host 已加载的有序模块列表。 |

### `ChengYuan.Core.Json`

核心原语的 Json 集成。

#### 关键类型

| 类型 | 说明 |
|---|---|
| `JsonModule` | 承载 Json 相关核心集成的可选模块。 |
| `StronglyTypedIdJsonConverterFactory` | 为 System.Text.Json 注册强类型 Id 转换器。 |

### `ChengYuan.Core.Validation`

核心层默认校验管道。

#### 关键类型

| 类型 | 说明 |
|---|---|
| `ValidationModule` | 向服务集合注册默认对象校验器。 |
| `IValidationRule<TValue>` | 为特定输入模型贡献校验错误。 |
| `IObjectValidator<TValue>` | 聚合给定输入的全部规则并返回校验结果。 |
| `ValidationResult` | 承载校验错误，而不耦合传输层或持久化层。 |

### `ChengYuan.Core.Localization`

核心契约的错误与资源本地化缝隙。

#### 关键类型

| 类型 | 说明 |
|---|---|
| `LocalizationModule` | 向服务集合注册默认错误本地化器。 |
| `ILocalizationResource` | 按错误码解析本地化字符串的资源缝隙。 |
| `IErrorLocalizer` | 以资源优先、原始消息兜底的方式本地化 `ResultError`。 |

### `ChengYuan.Core.Data`

Repository、工作单元与过滤器缝隙的 provider 无关数据契约。

#### 关键类型

| 类型 | 说明 |
|---|---|
| `DataModule` | 注册默认的开放泛型数据过滤器实现。 |
| `IDataFilter<TFilter>` | 可按异步流启用或禁用的环境式过滤器缝隙。 |
| `SoftDeleteFilter` | 面向软删除 provider 的标记过滤器。 |
| `MultiTenantFilter` | 面向租户隔离 provider 的标记过滤器。 |
| `IDataTenantProvider` | 向数据基础设施暴露当前租户 Id 的 provider 无关缝隙。 |
| `ISoftDelete` | 可软删除实体的持久化侧 trait。 |
| `IMultiTenant` | 按租户 Id 归属的实体持久化侧 trait。 |
| `IReadOnlyRepository<TEntity, TId>` | 与 provider 无关的聚合根读取契约。 |
| `IRepository<TEntity, TId>` | 建立在聚合根读取契约之上的最小变更契约。 |
| `IUnitOfWork` | 将持久化提交延后给 provider 实现的工作单元契约。 |
| `IUnitOfWorkAccessor` | 供 provider 实现传播当前工作单元的环境式访问器。 |

### `ChengYuan.Core.EntityFrameworkCore`

核心原语的 EF Core 集成。

#### 关键类型

| 类型 | 说明 |
|---|---|
| `StronglyTypedIdValueConverter<TStronglyTypedId, TValue>` | 在强类型 Id 与 provider value 之间做转换。 |
| `DbContextUnitOfWork` | 将 EF Core `DbContext` 适配为 provider 无关的 `IUnitOfWork` 契约。 |
| `EfRepository<TDbContext, TEntity, TId>` | 基于 `DbContext` 实现聚合根仓储契约的 provider，并在对应过滤器启用时识别软删除与租户查询。 |
| `EntityFrameworkCoreServiceCollectionExtensions` | 为指定 `DbContext` 向 DI 注册 EF 驱动的工作单元与仓储服务。 |

### `ChengYuan.ExecutionContext.Abstractions`

运行时执行上下文契约。

#### 关键类型

| 类型 | 说明 |
|---|---|
| `ICurrentUser` | 当前用户身份的只读视图。 |
| `ICurrentUserAccessor` | 供传输层和基础设施使用的作用域访问器。 |
| `ICurrentCorrelation` | 当前相关性标识访问接口。 |
| `ICurrentCorrelationAccessor` | 基于作用域的相关性访问器。 |
| `IClock` | 应用和框架代码使用的时间抽象。 |

### `ChengYuan.MultiTenancy.Abstractions`

运行时租户上下文契约。

#### 关键类型

| 类型 | 说明 |
|---|---|
| `ICurrentTenant` | 当前租户上下文的只读视图。 |
| `ICurrentTenantAccessor` | 用于切换租户作用域的访问器。 |
| `TenantInfo` | 对外暴露的不可变租户快照。 |

### `ChengYuan.TenantManagement`

应用层 tenant 目录管理能力。

#### 关键类型

| 类型 | 说明 |
|---|---|
| `TenantManagementModule` | 在现有 multi-tenancy runtime 之上注册 tenant 管理服务。 |
| `TenantRecord` | 携带租户 id、名称和启用状态的不可变租户记录。 |
| `ITenantStore` | 负责存储和查询受管 tenant。 |
| `ITenantReader` | 提供按 id、名称和列表读取 tenant 的能力。 |
| `ITenantManager` | 面向应用层的 tenant 创建、更新与移除服务。 |
| `InMemoryTenantStore` | 面向测试和轻量宿主的内存型 tenant 目录存储。 |
| `AddInMemoryTenantManagement()` | 将内存型 tenant 存储同时注册为 store 和 reader。 |

### `ChengYuan.TenantManagement.Persistence`

基于 EF Core 的 tenant 目录持久化 facet。

#### 关键类型

| 类型 | 说明 |
|---|---|
| `TenantManagementPersistenceModule` | 在 TenantManagement 应用模块之上注册基于 EF 的 tenant store。 |
| `TenantManagementDbContext` | 面向共享物理数据库但独立模块边界的 tenant 目录 DbContext。 |
| `TenantEntity` | 携带规范化名称与软删除状态的 EF tenant 持久化模型。 |
| `EfTenantStore` | `ITenantStore` 的 EF Core 实现。 |
| `AddTenantManagementPersistenceDbContext()` | 让调用方按自己的 provider 和连接方式注册模块 DbContext。 |
| `AddTenantManagementPersistence()` | 注册基于 EF 的 tenant store 与 unit of work 服务。 |

### `ChengYuan.Identity`

第一个完整的应用层用户管理 bounded context。

#### 关键类型

| 类型 | 说明 |
|---|---|
| `IdentityModule` | 在 core runtime 模块之上注册 Identity 应用服务。 |
| `RoleRecord` | 通过应用契约暴露的不可变角色快照。 |
| `UserRecord` | 通过应用契约暴露的不可变用户快照。 |
| `IRoleReader` | 提供按 id、名称和有序列表读取角色的能力。 |
| `IRoleManager` | 在保证角色名唯一性的前提下创建、更新和移除角色。 |
| `IUserReader` | 提供按 id、用户名、邮箱以及有序列表读取用户的能力。 |
| `IUserManager` | 在保证用户名和邮箱唯一性的前提下创建、更新、移除用户并管理其角色分配。 |
| `IdentityRole` | 带规范化名称和软删除行为的角色聚合根。 |
| `IdentityUser` | 带规范化用户名、规范化邮箱和软删除行为的用户聚合根。 |
| `IdentityUserRole` | 由领域模型拥有的用户-角色分配记录。 |
| `IIdentityRoleRepository` | IdentityRole 的领域仓储契约。 |
| `IIdentityUserRepository` | IdentityUser 的领域仓储契约。 |
| `RoleManager` | 负责协调角色唯一性、生命周期以及角色删除时分配清理的应用服务。 |
| `UserManager` | 负责协调唯一性校验和持久化的应用服务。 |

### `ChengYuan.Identity.Persistence`

基于 EF Core 的 identity user 持久化 facet。

#### 关键类型

| 类型 | 说明 |
|---|---|
| `IdentityPersistenceModule` | 在 Identity 应用模块之上注册基于 EF 的 identity user 仓储。 |
| `IdentityDbContext` | 面向共享物理数据库但独立模块边界的 identity user DbContext。 |
| `IdentityRoleConfiguration` | IdentityRole 聚合的 EF Core 模型配置。 |
| `IdentityUserConfiguration` | IdentityUser 聚合的 EF Core 模型配置。 |
| `IdentityUserRoleConfiguration` | 用户-角色分配表的 EF Core 模型配置。 |
| `EfIdentityRoleRepository` | `IIdentityRoleRepository` 的 EF Core 实现。 |
| `EfIdentityUserRepository` | `IIdentityUserRepository` 的 EF Core 实现。 |
| `AddIdentityDbContext()` | 让调用方按自己的 provider 和连接方式注册模块 DbContext。 |
| `AddIdentityPersistence()` | 注册基于 EF 的 identity 仓储与 unit of work 服务。 |

### `ChengYuan.Identity.Web`

Identity 的最小 HTTP 管理 facet。

#### 关键类型

| 类型 | 说明 |
|---|---|
| `IdentityWebModule` | 在 Identity 应用模块之上注册 Identity Web facet。 |
| `MapIdentityManagementEndpoints()` | 为用户、角色以及用户-角色分配操作映射最小 HTTP 管理面。 |

### `ChengYuan.Authorization`

基于定义的运行时权限判定系统。

#### 关键类型

| 类型 | 说明 |
|---|---|
| `AuthorizationModule` | 向服务集合注册默认权限定义管理器与权限判定管线。 |
| `PermissionScope` | 声明权限授予来自全局、租户还是用户层。 |
| `PermissionDefinition` | 定义带默认授予行为、显示元数据与描述的权限。 |
| `IPermissionDefinitionManager` | 负责注册和解析具名权限定义。 |
| `IPermissionChecker` | 基于既定 provider 顺序判定权限是否被授予。 |
| `IPermissionGrantProvider` | 为用户、租户或全局等来源提供权限授予结果。 |
| `InMemoryPermissionsBuilder` | 用于预加载全局、租户和用户权限授予结果的 fluent builder。 |
| `AddInMemoryPermissions()` | 为测试和简单宿主启动场景注册内存型权限 provider。 |
| `PermissionContext` | 将当前租户、用户、相关性与认证状态带入权限判定流程。 |
| `PermissionGrant` | 承载已解析的授予结果及其 provider 来源。 |

### `ChengYuan.Auditing`

环境式审计作用域与基于 sink 的审计采集系统。

#### 关键类型

| 类型 | 说明 |
|---|---|
| `AuditingModule` | 向服务集合注册默认审计作用域工厂。 |
| `AuditLogEntry` | 承载时间、环境上下文、成功状态与自定义属性的可变审计负载。 |
| `IAuditScopeFactory` | 创建审计作用域，并提供包装异步操作的辅助方法。 |
| `IAuditScope` | 表示可标记成功或失败并附加自定义属性的活动审计作用域。 |
| `IAuditLogContributor` | 在分发到 sink 之前对审计条目做补充。 |
| `IAuditLogSink` | 接收已完成的审计条目，用于持久化、导出或诊断。 |
| `InMemoryAuditLogCollector` | 在测试和轻量宿主中以内存方式存储完成后的审计条目。 |
| `AddInMemoryAuditing()` | 注册由 collector 支撑的内存型审计 sink。 |

### `ChengYuan.AuditLogging`

应用层审计日志持久化与读取能力。

#### 关键类型

| 类型 | 说明 |
|---|---|
| `AuditLoggingModule` | 将 framework 审计 sink 接到应用层审计日志存储。 |
| `AuditLogRecord` | 由应用层存储持久化的不可变审计日志快照。 |
| `IAuditLogStore` | 负责追加并列出已持久化的审计日志记录。 |
| `IAuditLogReader` | 提供对已存储审计日志记录的读取能力。 |
| `InMemoryAuditLogStore` | 面向测试和轻量宿主的内存型应用存储。 |
| `AddInMemoryAuditLogging()` | 将内存型审计日志存储同时注册为 store 和 reader。 |

### `ChengYuan.AuditLogging.Persistence`

基于 EF Core 的 audit log 持久化 facet。

#### 关键类型

| 类型 | 说明 |
|---|---|
| `AuditLoggingPersistenceModule` | 在 AuditLogging 应用模块之上注册基于 EF 的 audit log store。 |
| `AuditLoggingDbContext` | 面向共享物理数据库但独立模块边界的 audit log DbContext。 |
| `AuditLogEntity` | 包含自定义属性序列化结果的 EF audit log 持久化模型。 |
| `EfAuditLogStore` | `IAuditLogStore` 的 EF Core 实现。 |
| `AddAuditLoggingPersistenceDbContext()` | 让调用方按自己的 provider 和连接方式注册模块 DbContext 与 DbContext factory。 |
| `AddAuditLoggingPersistence()` | 注册基于 EF 的 audit log store 与 unit of work 服务。 |

### `ChengYuan.FeatureManagement`

应用层特性值存储与管理能力。

#### 关键类型

| 类型 | 说明 |
|---|---|
| `FeatureManagementModule` | 将应用层特性值管理接到 framework 特性 provider。 |
| `FeatureValueRecord` | 按全局、租户或用户范围存储的不可变特性记录。 |
| `IFeatureValueStore` | 负责存储和查询应用层特性值记录。 |
| `IFeatureValueReader` | 提供对已存储特性值记录的读取能力。 |
| `IFeatureValueManager` | 面向应用层的特性值设置与移除服务。 |
| `InMemoryFeatureValueStore` | 面向测试和轻量宿主的内存型特性值存储。 |
| `AddInMemoryFeatureManagement()` | 将内存型特性值存储同时注册为 store 和 reader。 |

### `ChengYuan.FeatureManagement.Persistence`

基于 EF Core 的 feature value 持久化 facet。

#### 关键类型

| 类型 | 说明 |
|---|---|
| `FeatureManagementPersistenceModule` | 在 FeatureManagement 应用模块之上注册基于 EF 的 feature value store。 |
| `FeatureManagementDbContext` | 面向共享物理数据库但独立模块边界的 feature value DbContext。 |
| `FeatureValueEntity` | 面向全局、租户和用户范围的 EF feature value 持久化模型。 |
| `EfFeatureValueStore` | `IFeatureValueStore` 的 EF Core 实现。 |
| `AddFeatureManagementPersistenceDbContext()` | 让调用方按自己的 provider 和连接方式注册模块 DbContext 与 DbContext factory。 |
| `AddFeatureManagementPersistence()` | 注册基于 EF 的 feature value store 与 unit of work 服务。 |

### `ChengYuan.PermissionManagement`

应用层权限授予存储与管理能力。

#### 关键类型

| 类型 | 说明 |
|---|---|
| `PermissionManagementModule` | 将应用层权限授予管理接到 framework 授权 provider。 |
| `PermissionGrantRecord` | 按全局、租户或用户范围存储的不可变授权记录。 |
| `IPermissionGrantStore` | 负责存储和查询应用层权限授予记录。 |
| `IPermissionGrantReader` | 提供对已存储权限授予记录的读取能力。 |
| `IPermissionGrantManager` | 面向应用层的权限授予设置与移除服务。 |
| `InMemoryPermissionGrantStore` | 面向测试和轻量宿主的内存型权限授予存储。 |
| `AddInMemoryPermissionManagement()` | 将内存型权限授予存储同时注册为 store 和 reader。 |

### `ChengYuan.PermissionManagement.Persistence`

基于 EF Core 的 permission grant 持久化 facet。

#### 关键类型

| 类型 | 说明 |
|---|---|
| `PermissionManagementPersistenceModule` | 在 PermissionManagement 应用模块之上注册基于 EF 的 permission grant store。 |
| `PermissionManagementDbContext` | 面向共享物理数据库但独立模块边界的 permission grant DbContext。 |
| `PermissionGrantEntity` | 面向全局、租户和用户范围的 EF permission grant 持久化模型。 |
| `EfPermissionGrantStore` | `IPermissionGrantStore` 的 EF Core 实现。 |
| `AddPermissionManagementPersistenceDbContext()` | 让调用方按自己的 provider 和连接方式注册模块 DbContext 与 DbContext factory。 |
| `AddPermissionManagementPersistence()` | 注册基于 EF 的 permission grant store 与 unit of work 服务。 |

### `ChengYuan.SettingManagement`

应用层设置值存储与管理能力。

#### 关键类型

| 类型 | 说明 |
|---|---|
| `SettingManagementModule` | 将应用层设置值管理接到 framework 设置 provider。 |
| `SettingValueRecord` | 按全局、租户或用户范围存储的不可变设置记录。 |
| `ISettingValueStore` | 负责存储和查询应用层设置值记录。 |
| `ISettingValueReader` | 提供对已存储设置值记录的读取能力。 |
| `ISettingValueManager` | 面向应用层的设置值设置与移除服务。 |
| `InMemorySettingValueStore` | 面向测试和轻量宿主的内存型设置值存储。 |
| `AddInMemorySettingManagement()` | 将内存型设置值存储同时注册为 store 和 reader。 |

### `ChengYuan.SettingManagement.Persistence`

基于 EF Core 的 setting 值持久化 facet。

#### 关键类型

| 类型 | 说明 |
|---|---|
| `SettingManagementPersistenceModule` | 在 SettingManagement 应用模块之上注册基于 EF 的 setting value store。 |
| `SettingManagementDbContext` | 面向共享物理数据库但独立模块边界的 setting 值 DbContext。 |
| `SettingValueEntity` | 面向全局、租户和用户范围的 EF setting 值持久化模型。 |
| `EfSettingValueStore` | `ISettingValueStore` 的 EF Core 实现。 |
| `AddSettingManagementPersistenceDbContext()` | 让调用方按自己的 provider 和连接方式注册模块 DbContext 与 DbContext factory。 |
| `AddSettingManagementPersistence()` | 注册基于 EF 的 setting store 与 unit of work 服务。 |

### `ChengYuan.Settings`

基于定义的运行时设置解析系统。

#### 关键类型

| 类型 | 说明 |
|---|---|
| `SettingsModule` | 向服务集合注册默认设置定义管理器与解析管线。 |
| `SettingScope` | 声明设置值来自全局、租户还是用户层。 |
| `SettingDefinition` | 定义带默认值、显示元数据与描述的强类型设置。 |
| `ISettingDefinitionManager` | 负责注册和解析具名设置定义。 |
| `ISettingProvider` | 基于当前用户与租户上下文，按既定 provider 顺序解析强类型设置值。 |
| `ISettingValueProvider` | 为用户、租户或全局等来源提供设置值。 |
| `InMemorySettingsBuilder` | 用于预加载全局、租户和用户设置值的 fluent builder。 |
| `AddInMemorySettings()` | 为测试和简单宿主启动场景注册内存型设置值 provider。 |
| `SettingContext` | 将当前租户、用户与相关性信息带入设置解析流程。 |
| `SettingValue` | 承载已解析的原始值及其 provider 来源。 |

### `ChengYuan.Features`

基于定义的运行时功能特性求值系统。

#### 关键类型

| 类型 | 说明 |
|---|---|
| `FeaturesModule` | 向服务集合注册默认 feature 定义管理器与求值管线。 |
| `FeatureScope` | 声明 feature 值来自全局、租户还是用户层。 |
| `FeatureDefinition` | 定义带默认值、显示元数据与描述的强类型 feature。 |
| `IFeatureDefinitionManager` | 负责注册和解析具名 feature 定义。 |
| `IFeatureChecker` | 基于既定 provider 顺序解析强类型 feature 值，并提供布尔启用判定。 |
| `IFeatureValueProvider` | 为用户、租户或全局等来源提供 feature 值。 |
| `InMemoryFeaturesBuilder` | 用于预加载全局、租户和用户 feature 值的 fluent builder。 |
| `AddInMemoryFeatures()` | 为测试和简单宿主启动场景注册内存型 feature value provider。 |
| `FeatureContext` | 将当前租户、用户与相关性信息带入 feature 求值流程。 |
| `FeatureValue` | 承载已解析的原始值及其 provider 来源。 |

#### 示例

```csharp
builder.Services.AddModule<WebHostModule>();

options.Converters.Add(new StronglyTypedIdJsonConverterFactory());

var validationResult = validator.Validate(command);
var errorMessage = errorLocalizer.Localize(validationResult.Errors[0]);

using (softDeleteFilter.Disable())
{
	var deletedWorkspace = await repository.FindAsync(workspaceId, cancellationToken);
}

using (unitOfWorkAccessor.Change(unitOfWork))
{
	await unitOfWorkAccessor.Current!.SaveChangesAsync(cancellationToken);
}
```

---

> **模板使用提示**：随着契约稳定，按模块逐步扩充本页。也可以考虑使用 [DocFX](https://dotnet.github.io/docfx/) 或 [xmldoc2md](https://github.com/nicolestandifer3/xmldoc2md-csharp) 从 XML 注释生成 API 参考。
