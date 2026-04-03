# API 参考

## 当前公开接口面

### `ChengYuan.Core`

基础结果模型、标识与 DDD 原语。

#### 关键类型

| 类型 | 说明 |
|---|---|
| `Result` 与 `Result<TValue>` | 供应用层与框架层共享的成功/失败模型。 |
| `ResultError` | 带错误码、消息与失败类别的稳定错误形状。 |
| `StronglyTypedId<TValue>` | 框架内使用的类型安全 Id 基类。 |
| `Entity<TId>` | 基于身份相等性的实体基类。 |
| `AggregateRoot<TId>` | 同时跟踪领域事件的实体基类。 |
| `ValueObject` | 提供结构化相等性的值对象基类。 |
| `IDomainEvent` | 聚合领域事件的标记抽象。 |
| `IClock` | 运行时与基础设施代码使用的时间抽象。 |

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
