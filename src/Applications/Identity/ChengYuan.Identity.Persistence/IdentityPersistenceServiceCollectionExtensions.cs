using ChengYuan.Core.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.Identity;

public static class IdentityPersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityPersistence(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddEntityFrameworkCoreDataAccess<IdentityDbContext>();
        services.AddEfRepository<IdentityDbContext, IdentityRole, Guid>();
        services.AddEfRepository<IdentityDbContext, IdentityUser, Guid>();
        services.TryAddScoped<IIdentityRoleRepository, EfIdentityRoleRepository>();
        services.TryAddScoped<IIdentityUserRepository, EfIdentityUserRepository>();

        return services;
    }

    public static IServiceCollection AddIdentityDbContext(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDbContext)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureDbContext);

        services.AddDbContext<IdentityDbContext>(configureDbContext);

        return services;
    }
}
