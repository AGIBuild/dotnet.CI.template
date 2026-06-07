using System.Net;
using System.Net.Http.Json;
using ChengYuan.Core.Data;
using ChengYuan.EntityFrameworkCore;
using ChengYuan.Identity;
using ChengYuan.WebHost;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class DataSeedingIntegrationTests
{
    [Fact]
    public async Task DataSeeding_ShouldSeedAdminUserAndRole()
    {
        await using var app = await CreateApplicationAsync();

        var roleReader = app.Services.GetRequiredService<IRoleReader>();
        var adminRole = await roleReader.FindByNameAsync("admin", TestContext.Current.CancellationToken);
        adminRole.ShouldNotBeNull();
        adminRole.Name.ShouldBe("admin");

        var userReader = app.Services.GetRequiredService<IUserReader>();
        var adminUser = await userReader.FindByUserNameAsync("admin", TestContext.Current.CancellationToken);
        adminUser.ShouldNotBeNull();
        adminUser.UserName.ShouldBe("admin");
        adminUser.Email.ShouldBe("admin@chengyuan.dev");
        adminUser.RoleIds.ShouldContain(adminRole.Id);
    }

    [Fact]
    public async Task DataSeeding_ShouldBeIdempotent()
    {
        await using var app = await CreateApplicationAsync();

        var seeder = app.Services.GetRequiredService<IDataSeeder>();
        await seeder.SeedAsync(new DataSeedContext(), TestContext.Current.CancellationToken);
        await seeder.SeedAsync(new DataSeedContext(), TestContext.Current.CancellationToken);

        var userReader = app.Services.GetRequiredService<IUserReader>();
        var users = await userReader.GetListAsync(TestContext.Current.CancellationToken);
        users.Count(u => u.UserName == "admin").ShouldBe(1);
    }

    [Fact]
    public async Task DataSeeding_ShouldAllowAdminToLoginOverHttp()
    {
        await using var app = await CreateApplicationAsync();
        var client = app.GetTestClient();

        var loginResponse = await client.PostAsJsonAsync("/api/v1/identity/login", new LoginRequest
        {
            UserName = "admin",
            Password = TestAdminPassword
        }, TestContext.Current.CancellationToken);

        loginResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var token = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>(TestContext.Current.CancellationToken);
        token.ShouldNotBeNull();
        token.AccessToken.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task DataSeeding_ShouldNotCreateAdminUser_WhenAdminSeedIsDisabled()
    {
        await using var app = await CreateApplicationAsync(configureAdminSeed: false);

        var userReader = app.Services.GetRequiredService<IUserReader>();
        var adminUser = await userReader.FindByUserNameAsync("admin", TestContext.Current.CancellationToken);

        adminUser.ShouldBeNull();
    }

    private const string TestAdminPassword = "SeededAdminPass123!";

    private static async Task<WebApplication> CreateApplicationAsync(bool configureAdminSeed = true)
    {
        var databaseName = $"seed-{Guid.NewGuid():N}";
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.UseDbContextOptions(options => options.UseInMemoryDatabase(databaseName));
        if (configureAdminSeed)
        {
            builder.Configuration["Identity:Admin:SeedEnabled"] = "true";
            builder.Configuration["Identity:Admin:UserName"] = "admin";
            builder.Configuration["Identity:Admin:Email"] = "admin@chengyuan.dev";
            builder.Configuration["Identity:Admin:Password"] = TestAdminPassword;
            builder.Configuration["Identity:Admin:RoleName"] = "admin";
        }
        builder.AddTestWebHost();
        builder.Services.TryAddTransient<IDataSeeder, DataSeeder>();
        builder.Services.AddHostedService<DataSeedingHostedService>();

        var app = builder.Build();
        app.UseWebHostComposition();
        await app.StartAsync();

        return app;
    }
}
