using System;
using System.Threading;
using System.Threading.Tasks;
using ChengYuan.Core.Modularity;
using ChengYuan.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class MultiTenancyResolverTests
{
    private static readonly Guid TenantA = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid TenantB = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    [Fact]
    public async Task Resolver_ShouldReturnUnresolved_WhenNoContributorsRegistered()
    {
        var resolver = BuildResolver(options => { });

        var result = await resolver.ResolveAsync(TestContext.Current.CancellationToken);

        result.TenantId.ShouldBeNull();
        result.TenantName.ShouldBeNull();
    }

    [Fact]
    public async Task Resolver_ShouldReturnFirstSuccessfulContributor()
    {
        var resolver = BuildResolver(options =>
        {
            options.Contributors.Add(typeof(UnresolvedContributor));
            options.Contributors.Add(typeof(TenantAContributor));
            options.Contributors.Add(typeof(TenantBContributor));
        });

        var result = await resolver.ResolveAsync(TestContext.Current.CancellationToken);

        result.TenantId.ShouldBe(TenantA);
        result.TenantName.ShouldBe("tenant-a");
    }

    [Fact]
    public async Task Resolver_ShouldSkipContributorsAfterFirstResolution()
    {
        var callTracker = new CallTracker();

        var resolver = BuildResolver(
            options =>
            {
                options.Contributors.Add(typeof(TenantAContributor));
                options.Contributors.Add(typeof(TrackingContributor));
            },
            services => services.AddSingleton(callTracker));

        var result = await resolver.ResolveAsync(TestContext.Current.CancellationToken);

        result.TenantId.ShouldBe(TenantA);
        callTracker.WasCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task Resolver_ShouldRunContributorsInConfiguredOrder()
    {
        var resolver = BuildResolver(options =>
        {
            options.Contributors.Add(typeof(TenantBContributor));
        });

        var result = await resolver.ResolveAsync(TestContext.Current.CancellationToken);

        result.TenantId.ShouldBe(TenantB);
        result.TenantName.ShouldBe("tenant-b");
    }

    [Fact]
    public async Task Resolver_ShouldUseFallback_WhenNoContributorResolves()
    {
        var fallbackId = Guid.NewGuid();
        var resolver = BuildResolver(options =>
        {
            options.Contributors.Add(typeof(UnresolvedContributor));
            options.FallbackTenantId = fallbackId;
            options.FallbackTenantName = "fallback";
        });

        var result = await resolver.ResolveAsync(TestContext.Current.CancellationToken);

        result.TenantId.ShouldBe(fallbackId);
        result.TenantName.ShouldBe("fallback");
    }

    [Fact]
    public async Task Resolver_ShouldNotUseFallback_WhenContributorResolves()
    {
        var resolver = BuildResolver(options =>
        {
            options.Contributors.Add(typeof(TenantAContributor));
            options.FallbackTenantId = Guid.NewGuid();
            options.FallbackTenantName = "fallback";
        });

        var result = await resolver.ResolveAsync(TestContext.Current.CancellationToken);

        result.TenantId.ShouldBe(TenantA);
        result.TenantName.ShouldBe("tenant-a");
    }

    [Fact]
    public async Task Resolver_ShouldReturnUnresolved_WhenAllContributorsFail()
    {
        var resolver = BuildResolver(options =>
        {
            options.Contributors.Add(typeof(UnresolvedContributor));
            options.Contributors.Add(typeof(UnresolvedContributor));
        });

        var result = await resolver.ResolveAsync(TestContext.Current.CancellationToken);

        result.TenantId.ShouldBeNull();
        result.TenantName.ShouldBeNull();
    }

    [Fact]
    public async Task Resolver_ShouldReturnResolvedOutcome_WhenContributorResolves()
    {
        var resolver = BuildResolver(options =>
        {
            options.Contributors.Add(typeof(TenantAContributor));
        });

        var result = await resolver.ResolveAsync(TestContext.Current.CancellationToken);

        result.Outcome.ShouldBe(TenantResolveOutcome.Resolved);
        result.TenantId.ShouldBe(TenantA);
    }

    [Fact]
    public async Task Resolver_ShouldReturnUnresolvedOutcome_WhenNothingResolves()
    {
        var resolver = BuildResolver(options =>
        {
            options.Contributors.Add(typeof(UnresolvedContributor));
        });

        var result = await resolver.ResolveAsync(TestContext.Current.CancellationToken);

        result.Outcome.ShouldBe(TenantResolveOutcome.Unresolved);
    }

    [Fact]
    public async Task Resolver_ShouldNormalizeTenantNameThroughStore()
    {
        var tenantId = Guid.NewGuid();
        var resolver = BuildResolver(
            options => { },
            services =>
            {
                services.AddSingleton<ITenantResolutionStore>(
                    new InMemoryStore(new TenantResolutionRecord(tenantId, "acme", true)));
                services.AddSingleton<ITenantResolutionSource>(
                    new NameCandidateSource("acme"));
            });

        var result = await resolver.ResolveAsync(TestContext.Current.CancellationToken);

        result.Outcome.ShouldBe(TenantResolveOutcome.Resolved);
        result.TenantId.ShouldBe(tenantId);
        result.TenantName.ShouldBe("acme");
    }

    [Fact]
    public async Task Resolver_ShouldReturnNotFound_WhenNameCandidateNotInStore()
    {
        var resolver = BuildResolver(
            options => { },
            services =>
            {
                services.AddSingleton<ITenantResolutionStore>(new InMemoryStore());
                services.AddSingleton<ITenantResolutionSource>(
                    new NameCandidateSource("unknown"));
            });

        var result = await resolver.ResolveAsync(TestContext.Current.CancellationToken);

        result.Outcome.ShouldBe(TenantResolveOutcome.NotFound);
    }

    [Fact]
    public async Task Resolver_ShouldReturnInactive_WhenTenantIsDisabled()
    {
        var tenantId = Guid.NewGuid();
        var resolver = BuildResolver(
            options => { },
            services =>
            {
                services.AddSingleton<ITenantResolutionStore>(
                    new InMemoryStore(new TenantResolutionRecord(tenantId, "disabled", false)));
                services.AddSingleton<ITenantResolutionSource>(
                    new NameCandidateSource("disabled"));
            });

        var result = await resolver.ResolveAsync(TestContext.Current.CancellationToken);

        result.Outcome.ShouldBe(TenantResolveOutcome.Inactive);
        result.TenantId.ShouldBe(tenantId);
    }

    [Fact]
    public async Task Resolver_ShouldNotUseFallback_WhenCandidateWasSuppliedButNotFound()
    {
        var fallbackId = Guid.NewGuid();
        var resolver = BuildResolver(
            options =>
            {
                options.FallbackTenantId = fallbackId;
                options.FallbackTenantName = "fallback";
            },
            services =>
            {
                services.AddSingleton<ITenantResolutionStore>(new InMemoryStore());
                services.AddSingleton<ITenantResolutionSource>(
                    new NameCandidateSource("unknown"));
            });

        var result = await resolver.ResolveAsync(TestContext.Current.CancellationToken);

        result.Outcome.ShouldBe(TenantResolveOutcome.NotFound);
        result.TenantId.ShouldNotBe(fallbackId);
    }

    private static ITenantResolver BuildResolver(
        Action<TenantResolveOptions> configureOptions,
        Action<IServiceCollection>? configureServices = null)
    {
        var services = new ServiceCollection();
        services.AddModule<ResolverTestModule>();
        services.Configure(configureOptions);
        configureServices?.Invoke(services);

        // Register test contributor types so DI can resolve them
        services.AddTransient<UnresolvedContributor>();
        services.AddTransient<TenantAContributor>();
        services.AddTransient<TenantBContributor>();
        services.AddTransient<TrackingContributor>();

        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<ITenantResolver>();
    }

    [DependsOn(typeof(MultiTenancyModule))]
    private sealed class ResolverTestModule : FrameworkCoreModule;

    private sealed class UnresolvedContributor : ITenantResolveContributor
    {
        public string Name => "Unresolved";

        public Task ResolveAsync(TenantResolveContext context, CancellationToken cancellationToken = default)
        {
            // Intentionally does nothing — leaves context unresolved
            return Task.CompletedTask;
        }
    }

    private sealed class TenantAContributor : ITenantResolveContributor
    {
        public string Name => "TenantA";

        public Task ResolveAsync(TenantResolveContext context, CancellationToken cancellationToken = default)
        {
            context.TenantId = TenantA;
            context.TenantName = "tenant-a";
            context.HasResolved = true;
            return Task.CompletedTask;
        }
    }

    private sealed class TenantBContributor : ITenantResolveContributor
    {
        public string Name => "TenantB";

        public Task ResolveAsync(TenantResolveContext context, CancellationToken cancellationToken = default)
        {
            context.TenantId = TenantB;
            context.TenantName = "tenant-b";
            context.HasResolved = true;
            return Task.CompletedTask;
        }
    }

    private sealed class CallTracker
    {
        public bool WasCalled { get; set; }
    }

    private sealed class TrackingContributor(CallTracker tracker) : ITenantResolveContributor
    {
        public string Name => "Tracking";

        public Task ResolveAsync(TenantResolveContext context, CancellationToken cancellationToken = default)
        {
            tracker.WasCalled = true;
            return Task.CompletedTask;
        }
    }

    private sealed class NameCandidateSource(string tenantName) : ITenantResolutionSource
    {
        public int Order => 50;
        public bool IsAvailable(IServiceProvider serviceProvider) => true;

        public ValueTask PopulateAsync(
            TenantResolveContext context,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken = default)
        {
            context.TenantName = tenantName;
            context.SourceName = nameof(NameCandidateSource);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class InMemoryStore(params TenantResolutionRecord[] records) : ITenantResolutionStore
    {
        public Task<TenantResolutionRecord?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(records.FirstOrDefault(r => r.Id == id));

        public Task<TenantResolutionRecord?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
            => Task.FromResult(records.FirstOrDefault(r =>
                string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase)));
    }
}
