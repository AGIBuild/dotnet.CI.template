using ChengYuan.Core.DependencyInjection;

namespace ChengYuan.Identity;

[ExposeServices(typeof(IPasswordHasher))]
public sealed class BcryptPasswordHasher : IPasswordHasher, ISingletonService
{
    public string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        return BCrypt.Net.BCrypt.EnhancedHashPassword(password);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        return BCrypt.Net.BCrypt.EnhancedVerify(password, passwordHash);
    }
}
