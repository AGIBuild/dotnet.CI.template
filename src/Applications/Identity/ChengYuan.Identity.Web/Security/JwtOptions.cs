using System.ComponentModel.DataAnnotations;

namespace ChengYuan.Identity;

public sealed class JwtOptions
{
    [Required]
    [MinLength(32)]
    public string SecretKey { get; set; } = string.Empty;

    [Required]
    public string Issuer { get; set; } = "chengyuan";

    [Required]
    public string Audience { get; set; } = "chengyuan-api";

    [Range(1, 1440)]
    public int AccessTokenExpirationMinutes { get; set; } = 60;
}
