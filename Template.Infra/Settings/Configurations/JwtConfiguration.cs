namespace Template.Infra.Settings.Configurations;

public class JwtConfiguration
{
    public const string Key = "JwtSettings";
    public string? Secret { get; set; }
    public string? Issuer { get; set; }
    // Audience é dinâmico: vem do TenantCache (Client.Url) ou CorsSettings:AllowedOrigins (primeiro da lista)
    public int ExpiryMinutes { get; set; }
    public int RefreshTokenDays { get; set; }
}