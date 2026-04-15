using System.Reflection;
using ChengYuan.Auditing;
using ChengYuan.Core.Data;
using ChengYuan.Core.Data.Auditing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class AuditableEntityTypeResolverTests
{
    [Fact]
    public void IsAuditable_ShouldReturnFalse_ForPlainEntity()
    {
        var resolver = CreateResolver();

        resolver.IsAuditable(typeof(PlainEntity)).ShouldBeFalse();
    }

    [Fact]
    public void IsAuditable_ShouldReturnTrue_ForEntityWithAuditedAttribute()
    {
        var resolver = CreateResolver();

        resolver.IsAuditable(typeof(AuditedEntity)).ShouldBeTrue();
    }

    [Fact]
    public void IsAuditable_ShouldReturnFalse_ForEntityWithDisableAuditingAttribute()
    {
        var resolver = CreateResolver();

        resolver.IsAuditable(typeof(DisabledAuditEntity)).ShouldBeFalse();
    }

    [Fact]
    public void IsAuditable_ShouldReturnFalse_WhenDisableAuditingOverridesAuditedAttribute()
    {
        var resolver = CreateResolver();

        resolver.IsAuditable(typeof(DisabledOverridesAuditedEntity)).ShouldBeFalse();
    }

    [Fact]
    public void IsAuditable_ShouldReturnTrue_ForICreationAuditedEntity()
    {
        var resolver = CreateResolver();

        resolver.IsAuditable(typeof(CreationAuditedEntity)).ShouldBeTrue();
    }

    [Fact]
    public void IsAuditable_ShouldReturnTrue_ForIAuditedEntity()
    {
        var resolver = CreateResolver();

        resolver.IsAuditable(typeof(FullyAuditedEntity)).ShouldBeTrue();
    }

    [Fact]
    public void IsAuditable_ShouldReturnTrue_WhenEntityHistorySelectorMatches()
    {
        var resolver = CreateResolver(options =>
        {
            options.EntityHistorySelectors.Add(type => type.Name.Contains("Selected", StringComparison.Ordinal));
        });

        resolver.IsAuditable(typeof(SelectedEntity)).ShouldBeTrue();
    }

    [Fact]
    public void IsPropertyAuditable_ShouldReturnFalse_ForPropertyWithDisableAuditingAttribute()
    {
        var resolver = CreateResolver();

        resolver.IsPropertyAuditable(typeof(EntityWithDisabledProperty), nameof(EntityWithDisabledProperty.Secret)).ShouldBeFalse();
    }

    [Fact]
    public void IsPropertyAuditable_ShouldReturnTrue_ForRegularProperty()
    {
        var resolver = CreateResolver();

        resolver.IsPropertyAuditable(typeof(EntityWithDisabledProperty), nameof(EntityWithDisabledProperty.Name)).ShouldBeTrue();
    }

    private static AuditableEntityTypeResolver CreateResolver(Action<AuditingOptions>? configure = null)
    {
        var auditingOptions = new AuditingOptions();
        configure?.Invoke(auditingOptions);
        return new AuditableEntityTypeResolver(Options.Create(auditingOptions));
    }

    private sealed class PlainEntity;

    [Audited]
    private sealed class AuditedEntity;

    [DisableAuditing]
    private sealed class DisabledAuditEntity;

    [Audited]
    [DisableAuditing]
    private sealed class DisabledOverridesAuditedEntity;

    private sealed class CreationAuditedEntity : ICreationAudited
    {
        public DateTimeOffset CreationTime { get; set; }
        public string? CreatorId { get; set; }
    }

    private sealed class FullyAuditedEntity : IAudited
    {
        public DateTimeOffset CreationTime { get; set; }
        public string? CreatorId { get; set; }
        public DateTimeOffset? LastModificationTime { get; set; }
        public string? LastModifierId { get; set; }
    }

    private sealed class SelectedEntity;

    [Audited]
    private sealed class EntityWithDisabledProperty
    {
        public string? Name { get; set; }

        [DisableAuditing]
        public string? Secret { get; set; }
    }
}
