using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ChengYuan.FrameworkKernel.Tests;

internal static class TestJwtTokenFactory
{
    public static string CreateAccessToken(Guid? tenantId = null, Guid? userId = null)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestHostBuilder.TestJwtSecretKey));
        var claims = new Dictionary<string, object>
        {
            [JwtRegisteredClaimNames.Sub] = (userId ?? Guid.NewGuid()).ToString(),
            [JwtRegisteredClaimNames.Name] = "tenantadmin",
            [JwtRegisteredClaimNames.Email] = "tenantadmin@test.com",
            [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString(),
        };

        if (tenantId.HasValue)
        {
            claims["tenant_id"] = tenantId.Value.ToString();
        }

        var now = DateTime.UtcNow;
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = "test",
            Audience = "test",
            Claims = claims,
            IssuedAt = now,
            NotBefore = now,
            Expires = now.AddMinutes(30),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}
