using ChengYuan.Core.Data;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public sealed class DataSeederTests
{
    [Fact]
    public async Task SeedAsync_ExecutesAllContributors()
    {
        var contributor1 = new TrackingContributor();
        var contributor2 = new TrackingContributor();
        var seeder = new DataSeeder([contributor1, contributor2], NullLogger<DataSeeder>.Instance);
        var context = new DataSeedContext();

        await seeder.SeedAsync(context, TestContext.Current.CancellationToken);

        contributor1.SeedCount.ShouldBe(1);
        contributor2.SeedCount.ShouldBe(1);
    }

    [Fact]
    public async Task SeedAsync_PassesContextToContributors()
    {
        var contributor = new TrackingContributor();
        var seeder = new DataSeeder([contributor], NullLogger<DataSeeder>.Instance);
        var tenantId = Guid.NewGuid();
        var context = new DataSeedContext(tenantId);

        await seeder.SeedAsync(context, TestContext.Current.CancellationToken);

        contributor.LastContext.ShouldNotBeNull();
        contributor.LastContext.TenantId.ShouldBe(tenantId);
    }

    [Fact]
    public async Task SeedAsync_WithNoContributors_DoesNotThrow()
    {
        var seeder = new DataSeeder([], NullLogger<DataSeeder>.Instance);

        await Should.NotThrowAsync(
            () => seeder.SeedAsync(new DataSeedContext(), TestContext.Current.CancellationToken).AsTask());
    }

    private sealed class TrackingContributor : IDataSeedContributor
    {
        public int SeedCount { get; private set; }
        public DataSeedContext? LastContext { get; private set; }

        public ValueTask SeedAsync(DataSeedContext context, CancellationToken cancellationToken = default)
        {
            SeedCount++;
            LastContext = context;
            return ValueTask.CompletedTask;
        }
    }
}
