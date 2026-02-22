namespace Template.Infra.Identity;

/// <summary>
/// Par de tokens (Access + Refresh) retornado pelo TokenService
/// </summary>
public record TokenPair(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpires,
    DateTime RefreshTokenExpires
);

public interface ITokenService
{
    /// <summary>
    /// Gera apenas o Access Token JWT (metodo legado para compatibilidade)
    /// </summary>
    string GenerateJwt(ContextUser user, List<string> roles, Guid tenantId);

    /// <summary>
    /// Valida o token e retorna o UserId
    /// </summary>
    string ValidateTokenGetUserId(string token, Guid tenantId);

    /// <summary>
    /// Gera par de tokens (Access + Refresh) e salva o Refresh Token na tabela AspNetUserTokens
    /// </summary>
    Task<TokenPair> GenerateTokenPairAsync(string userId, Guid tenantId);

    /// <summary>
    /// Renova os tokens usando o Refresh Token (Refresh Token Rotation)
    /// </summary>
    Task<TokenPair?> RefreshTokensAsync(string userId, string refreshToken, Guid tenantId);

    /// <summary>
    /// Revoga o Refresh Token do usuario (logout)
    /// </summary>
    Task RevokeRefreshTokenAsync(string userId);

    /// <summary>
    /// Extrai o UserId de um token JWT (mesmo expirado)
    /// </summary>
    string? ExtractUserIdFromToken(string token);
}
