using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using Template.Application.Common.Interfaces.Services;
using Template.Infra.Settings.Configurations;

namespace Template.Infra.Identity;

public class TokenService : ITokenService
{
    private readonly JwtConfiguration _jwtConfig;
    private readonly UserManager<ContextUser> _userManager;
    private readonly ITenantCacheService? _tenantCacheService;
    private readonly IConfiguration? _configuration;

    private const string REFRESH_TOKEN_PROVIDER = "Template";
    private const string REFRESH_TOKEN_NAME = "RefreshToken";

    public TokenService(
        IOptions<JwtConfiguration> jwtConfig,
        UserManager<ContextUser> userManager,
        ITenantCacheService? tenantCacheService = null,
        IConfiguration? configuration = null)
    {
        _jwtConfig = jwtConfig.Value ?? throw new ArgumentNullException(nameof(jwtConfig));
        _userManager = userManager;
        _tenantCacheService = tenantCacheService;
        _configuration = configuration;
    }

    public string GetTenantSpecificSecret(Guid tenantId)
    {
        return $"{tenantId}_{_jwtConfig.Secret}";
    }

    /// <summary>
    /// Gera um token JWT (método legado para compatibilidade)
    /// </summary>
    public string GenerateJwt(ContextUser user, List<string> roles, Guid tenantId)
    {
        var signingCredentials = GetSigningCredentials(tenantId);
        var claims = GetClaims(user, roles);
        var audience = GetAudienceAsync(tenantId).GetAwaiter().GetResult();
        return GenerateEncryptedToken(signingCredentials, claims, audience);
    }

    /// <summary>
    /// Gera um par de tokens (Access + Refresh) usando a infraestrutura do Identity
    /// </summary>
    public async Task<TokenPair> GenerateTokenPairAsync(string userId, Guid tenantId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            throw new InvalidOperationException($"User with ID '{userId}' not found.");

        var roles = (await _userManager.GetRolesAsync(user)).Select(r => r.ToUpper()).ToList();

        var accessTokenExpires = DateTime.UtcNow.AddMinutes(_jwtConfig.ExpiryMinutes);
        var refreshTokenExpires = DateTime.UtcNow.AddDays(_jwtConfig.RefreshTokenDays);

        // Gera Access Token (JWT) com Audience dinâmico
        var signingCredentials = GetSigningCredentials(tenantId);
        var claims = GetClaims(user, roles);
        var audience = await GetAudienceAsync(tenantId);
        var accessToken = GenerateEncryptedToken(signingCredentials, claims, audience);

        // Gera Refresh Token (opaco, seguro)
        var refreshToken = GenerateSecureRefreshToken();

        // Salva o Refresh Token usando o Identity (tabela AspNetUserTokens)
        await _userManager.RemoveAuthenticationTokenAsync(user, REFRESH_TOKEN_PROVIDER, REFRESH_TOKEN_NAME);
        await _userManager.SetAuthenticationTokenAsync(user, REFRESH_TOKEN_PROVIDER, REFRESH_TOKEN_NAME, refreshToken);

        return new TokenPair(
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            AccessTokenExpires: accessTokenExpires,
            RefreshTokenExpires: refreshTokenExpires
        );
    }

    /// <summary>
    /// Renova os tokens usando o Refresh Token (Refresh Token Rotation)
    /// </summary>
    public async Task<TokenPair?> RefreshTokensAsync(string userId, string refreshToken, Guid tenantId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return null;

        // Busca o Refresh Token armazenado na tabela AspNetUserTokens
        var storedToken = await _userManager.GetAuthenticationTokenAsync(user, REFRESH_TOKEN_PROVIDER, REFRESH_TOKEN_NAME);

        // Valida se o token enviado é igual ao armazenado
        if (string.IsNullOrEmpty(storedToken) || storedToken != refreshToken)
            return null;

        // Gera novo par de tokens (Refresh Token Rotation - invalida o anterior)
        return await GenerateTokenPairAsync(userId, tenantId);
    }

    /// <summary>
    /// Revoga o Refresh Token do usuário (usado no logout)
    /// </summary>
    public async Task RevokeRefreshTokenAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is not null)
            await _userManager.RemoveAuthenticationTokenAsync(user, REFRESH_TOKEN_PROVIDER, REFRESH_TOKEN_NAME);
    }

    /// <summary>
    /// Extrai o UserId de um token JWT (mesmo expirado)
    /// </summary>
    public string? ExtractUserIdFromToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();

            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                token = token["Bearer ".Length..];

            var jwt = handler.ReadJwtToken(token);
            return jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Valida o token JWT fornecido e retorna o ID do usuário, se válido.
    /// </summary>
    public string ValidateTokenGetUserId(string token, Guid tenantId)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return "Invalid token";
        }

        if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            token = token["Bearer ".Length..];
        }

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = GetValidationParameters(tenantId);

            ClaimsPrincipal principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "User ID not found";
        }
        catch (SecurityTokenException ex)
        {
            return $"Token validation failed: {ex.Message}";
        }
        catch (Exception ex)
        {
            return $"Unexpected error: {ex.Message}";
        }
    }

    private string GenerateEncryptedToken(SigningCredentials signingCredentials, IEnumerable<Claim> claims, string audience)
    {
        // Em Development: Issuer vem do appsettings
        // Em Production: Issuer é dinâmico (igual ao Audience)
        var isDevelopment = string.Equals(
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            "Development",
            StringComparison.OrdinalIgnoreCase);

        var issuer = isDevelopment ? _jwtConfig.Issuer : audience;

        var token = new JwtSecurityToken(
           issuer: issuer,
           audience: audience,
           claims: claims,
           expires: DateTime.UtcNow.AddMinutes(_jwtConfig.ExpiryMinutes),
           signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private SigningCredentials GetSigningCredentials(Guid tenantId)
    {
        var tenantSpecificSecret = GetTenantSpecificSecret(tenantId);
        var secretKey = Encoding.ASCII.GetBytes(tenantSpecificSecret);
        return new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256);
    }

    /// <summary>
    /// Cria uma lista de claims para o usuário fornecido
    /// </summary>
    private static IEnumerable<Claim> GetClaims(ContextUser user, List<string> roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // JWT ID único para auditoria
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.FullName ?? string.Empty),
            new(ClaimTypes.MobilePhone, user.PhoneNumber ?? string.Empty)
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        if (user.BypassIp)
            claims.Add(new Claim("scp", "full"));

        return claims;
    }

    /// <summary>
    /// Define os parâmetros de validação do token.
    /// </summary>
    private TokenValidationParameters GetValidationParameters(Guid tenantId)
    {
        var tenantSpecificSecret = GetTenantSpecificSecret(tenantId);
        var secretKey = Encoding.ASCII.GetBytes(tenantSpecificSecret);
        var audience = GetAudienceAsync(tenantId).GetAwaiter().GetResult();

        // Em Development: Issuer vem do appsettings
        // Em Production: Issuer é dinâmico (igual ao Audience)
        var isDevelopment = string.Equals(
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            "Development",
            StringComparison.OrdinalIgnoreCase);

        var issuer = isDevelopment ? _jwtConfig.Issuer : audience;

        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(secretKey),
            ClockSkew = TimeSpan.Zero
        };
    }

    /// <summary>
    /// Gera um token seguro de 64 bytes para Refresh Token
    /// </summary>
    private static string GenerateSecureRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    /// <summary>
    /// Obtém Audience dinâmico baseado no TenantID.
    /// - Tenant (TenantID != Guid.Empty): usa URL do cliente no cache
    /// - Core (TenantID == Guid.Empty): usa primeiro URL do CorsSettings:AllowedOrigins
    /// </summary>
    private async Task<string> GetAudienceAsync(Guid tenantId)
    {
        // Se é um tenant, busca URL do cache
        if (tenantId != Guid.Empty && _tenantCacheService != null)
        {
            var tenantUrl = await _tenantCacheService.GetTenantUrlByIdAsync(tenantId);
            if (!string.IsNullOrWhiteSpace(tenantUrl))
            {
                return NormalizeUrl(tenantUrl);
            }
        }

        // Core: usa primeiro URL do CorsSettings:AllowedOrigins
        if (_configuration != null)
        {
            var allowedOrigins = _configuration["CorsSettings:AllowedOrigins"];
            if (!string.IsNullOrWhiteSpace(allowedOrigins))
            {
                var firstOrigin = allowedOrigins
                    .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .FirstOrDefault()?.Trim();

                if (!string.IsNullOrWhiteSpace(firstOrigin))
                {
                    return NormalizeUrl(firstOrigin);
                }
            }
        }

        // Fallback: usa Issuer como Audience (comportamento-padrão)
        return _jwtConfig.Issuer;
    }

    /// <summary>
    /// Normaliza URL para usar como Issuer/Audience do JWT.
    /// Adiciona https:// se não tiver protocolo.
    /// </summary>
    private static string NormalizeUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return url;

        url = url.Trim();

        // Se já tem protocolo, retorna como está
        if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return url.TrimEnd('/');
        }

        // Adiciona https:// por padrão
        return $"https://{url}".TrimEnd('/');
    }
}
