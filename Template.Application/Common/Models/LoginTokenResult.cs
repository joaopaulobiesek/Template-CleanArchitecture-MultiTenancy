namespace Template.Application.Common.Models;

/// <summary>
/// Resultado do login com par de tokens (Access + Refresh)
/// </summary>
public class LoginTokenResult
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpires { get; set; }
    public DateTime RefreshTokenExpires { get; set; }

    public LoginTokenResult() { }

    public LoginTokenResult(string userId, string fullName, string email, string accessToken, string refreshToken, DateTime accessTokenExpires, DateTime refreshTokenExpires)
    {
        UserId = userId;
        FullName = fullName;
        Email = email;
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        AccessTokenExpires = accessTokenExpires;
        RefreshTokenExpires = refreshTokenExpires;
    }
}
