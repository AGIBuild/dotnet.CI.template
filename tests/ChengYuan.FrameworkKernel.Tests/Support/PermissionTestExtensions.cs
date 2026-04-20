using ChengYuan.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.FrameworkKernel.Tests;

internal static class PermissionTestExtensions
{
    /// <summary>
    /// Registers a permissive grant provider that grants all permissions
    /// for authenticated users. Intended for integration tests only.
    /// </summary>
    public static IServiceCollection AddPermissivePermissionGrants(this IServiceCollection services)
    {
        services.AddSingleton<IPermissionGrantProvider>(new PermissivePermissionGrantProvider());
        return services;
    }

    private sealed class PermissivePermissionGrantProvider : IPermissionGrantProvider
    {
        public string Name => "TestPermissive";

        public int Order => int.MaxValue;

        public ValueTask<PermissionGrantResult> CheckAsync(
            PermissionDefinition definition,
            PermissionContext context,
            CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(PermissionGrantResult.Granted);
        }
    }
}
